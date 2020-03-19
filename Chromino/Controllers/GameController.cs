using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Controllers
{
    [Authorize]
    public class GameController : CommonController
    {
        private static readonly Random Random = new Random();

        public GameController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        /// <summary>
        /// page de création de partie
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult New()
        {
            GetPlayerInfos();
            return View(null);
        }

        /// <summary>
        /// retour du formulaire de création de partie
        /// </summary>
        /// <param name="pseudos">pseudos des joueurs de la partie à créer</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult New(string[] pseudos)
        {
            GetPlayerInfos();
            if (pseudos == null || pseudos.Length == 0)
            {
                return View();
            }
            List<string> listPseudos = pseudos.ToList();
            listPseudos.Add(PlayerPseudo);
            listPseudos.Reverse();
            string[] pseudosNotNull = listPseudos.Where(c => c != null).ToArray();

            List<string> errors = new List<String>();
            List<Player> players = new List<Player>(8);
            for (int i = 0; i < pseudosNotNull.Length; i++)
            {
                Player player = PlayerDal.Details(pseudosNotNull[i]);
                if (player == null)
                    errors.Add($"Le joueur {pseudosNotNull[i]} n'existe pas");
                else if (players.Contains(player))
                    errors.Add($"Le joueur {pseudosNotNull[i]} est ajouté plusieurs fois dans la partie");
                else
                    players.Add(player);
            }
            if (errors.Count != 0)
            {
                ViewBag.errors = errors;
                return View(pseudos);
            }

            CreateGame(ref players, out int gameId);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        /// <summary>
        /// page de création de partie contre bots
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult NewAgainstBots()
        {
            return View();
        }

        /// <summary>
        /// retour du formulaire de création de partie contre bots
        /// </summary>
        /// <param name="botsNumber">nombre d'adversaires bots</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult NewAgainstBots(int botsNumber)
        {
            GetPlayerInfos();
            List<Player> players = new List<Player>(botsNumber + 1);
            players.Add(PlayerDal.Details(PlayerId));
            for (int iBot = 1; iBot <= botsNumber; iBot++)
                players.Add(PlayerDal.Details("Bot" + iBot));

            CreateGame(ref players, out int gameId);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        /// <summary>
        /// page de création de partie solo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult NewSingle()
        {
            GetPlayerInfos();
            List<Player> players = new List<Player> { PlayerDal.Details(PlayerId) };
            CreateGame(ref players, out int gameId);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        /// <summary>
        /// tente de jouer le chromino à l'emplacement choisit par le joueur
        /// </summary>
        /// <param name="playerId">id du joueur</param>
        /// <param name="gameId">id du jeu</param>
        /// <param name="chrominoId">id du chromino</param>
        /// <param name="x">abscisse (0 étant le Caméléon du premier chromino du jeu)</param>
        /// <param name="y">ordonnée (0 étant le Caméléon du premier chromino du jeu)</param>
        /// <param name="orientation">vertical, horizontal, etc</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Play(int playerId, int gameId, int chrominoId, int x, int y, Orientation orientation)
        {
            GetPlayerInfos();
            GameCore gameCore = new GameCore(Ctx, Env, gameId);
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chrominoId,
                XPosition = x,
                YPosition = y,
                Orientation = orientation,
            };
            PlayReturn playReturn = gameCore.Play(chrominoInGame, playerId);

            if (playReturn != PlayReturn.Ok)
                TempData["PlayReturn"] = playReturn; //todo voir si ajax doit appeler NextPlayerPlayIfBot

            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        /// <summary>
        /// Pioche un chromino
        /// </summary>
        /// <param name="playerId">id du joueur</param>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DrawChromino(int playerId, int gameId)
        {
            GetPlayerInfos();
            GameCore gameCore = new GameCore(Ctx, Env, gameId);
            int playersNumber = GamePlayerDal.PlayersNumber(gameId);
            GamePlayer gamePlayer = GamePlayerDal.Details(gameId, playerId);
            if (playerId == PlayerId && (!gamePlayer.PreviouslyDraw || playersNumber == 1))
            {
                gameCore.DrawChromino(playerId);
            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        /// <summary>
        /// Passe le tour du joueur
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PassTurn(int playerId, int gameId)
        {
            GetPlayerInfos();
            if (playerId == PlayerId)
            {
                GameCore gameCore = new GameCore(Ctx, Env, gameId);
                gameCore.PassTurn(playerId);
                //NextPlayerPlayIfBot(gameId, gameCore); todo voir si on doit appeler NextPlayerPlayIfBot
            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        /// <summary>
        /// affiche une partie 
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <returns></returns>
        public IActionResult Show(int id)
        {
            if (id == 0)
                return RedirectToAction("NotFound");

            GetPlayerInfos();
            //TODO passer l'essentiel de l'algo dans GameCore
            List<Player> players = GamePlayerDal.Players(id);
            int playersNumber = players.Count;
            Player playerTurn = GamePlayerDal.PlayerTurn(id);

            if (players.Where(x => x.Id == PlayerId).FirstOrDefault() != null || playerTurn.Bot) // identified player in the game or bot play
            {
                int chrominosInStackNumber = GameChrominoDal.InStack(id);

                Dictionary<string, int> pseudosChrominos = new Dictionary<string, int>();
                List<string> pseudos = new List<string>();
                foreach (Player player in players)
                {
                    pseudosChrominos.Add(player.UserName, GameChrominoDal.PlayerNumberChrominos(id, player.Id));
                    pseudos.Add(player.UserName);
                }
                Dictionary<string, Chromino> pseudos_lastChrominos = new Dictionary<string, Chromino>();
                foreach (var pseudo_chromino in pseudosChrominos)
                {
                    if (pseudo_chromino.Value == 1)
                        pseudos_lastChrominos.Add(pseudo_chromino.Key, GameChrominoDal.FirstChromino(id, GamePlayerDal.PlayerId(id, pseudo_chromino.Key)));
                }
                List<Chromino> identifiedPlayerChrominos = new List<Chromino>();
                if (GamePlayerDal.IsBots(id)) // s'il n'y a que des bots en jeu, on regarde la partie et leur main
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, playerTurn.Id);
                else
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, PlayerId);
                if (GameDal.IsFinished(id) && !GamePlayerDal.GetViewFinished(id, PlayerId))
                {
                    GamePlayerDal.SetViewFinished(id, PlayerId);
                    if (GamePlayerDal.GetWon(id, PlayerId) == null)
                        GamePlayerDal.SetWon(id, PlayerId, false);
                }
                GamePlayer gamePlayerTurn = GamePlayerDal.Details(id, playerTurn.Id);
                GamePlayer gamePlayerIdentified = GamePlayerDal.Details(id, PlayerId);
                List<Square> squares = SquareDal.List(id);
                List<int> botsId = PlayerDal.BotsId();
                List<ChrominoInGame> chrominosInGamePlayed = GameChrominoDal.ChrominosInGamePlayed(id);
                List<string> pseudoChrominosInGamePlayed = new List<string>();
                Game game = GameDal.Details(id);
                bool opponenentsAreBots = GamePlayerDal.IsOpponenentsAreBots(id, PlayerId);
                GameVM gameViewModel = new GameVM(game, squares, chrominosInStackNumber, pseudosChrominos, identifiedPlayerChrominos, playerTurn, gamePlayerTurn, gamePlayerIdentified, botsId, pseudos_lastChrominos, chrominosInGamePlayed, pseudos, opponenentsAreBots);
                return View(gameViewModel);
            }
            else
            {
                return RedirectToAction("NotFound");
            }
        }

        /// <summary>
        /// affiche à l'écran la prochaine partie à jouer
        /// </summary>
        /// <returns></returns>
        public IActionResult NextToPlay()
        {
            GetPlayerInfos();
            TempData["ShowInfos"] = true;
            int gameId = GamePlayerDal.FirstIdMultiGameToPlay(PlayerId);
            return gameId == 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// Page de partie non trouvée ou non autorisée
        /// </summary>
        /// <returns></returns>
        public IActionResult NotFound()
        {
            return View();
        }

        /// <summary>
        /// fait jouer un bot
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <param name="botId">Id du bot</param>
        /// <returns></returns>
        public IActionResult PlayBot(int id, int botId)
        {
            GameCore gamecore = new GameCore(Ctx, Env, id);
            gamecore.PlayBot(botId);
            return RedirectToAction("Show", "Game", new { id });
        }

        private void CreateGame(ref List<Player> players, out int gameId)
        {
            List<Player> randomPlayers = players.OrderBy(_ => Random.Next()).ToList();
            ChrominoDal.CreateChrominos();
            gameId = GameDal.Add().Id;
            GamePlayerDal.Add(gameId, randomPlayers);
            GameCore gamecore = new GameCore(Ctx, Env, gameId);
            gamecore.BeginGame(randomPlayers.Count);
        }
    }
}
