using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Controllers
{
    public class GameController : CommonController
    {
        public GameController(Context context) : base(context)
        {
        }

        /// <summary>
        /// page de création de partie
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult New()
        {
            GetPlayerInfosFromSession();
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
            GetPlayerInfosFromSession();

            if (pseudos == null || pseudos.Length == 0)
            {
                return View();
            }
            string[] pseudosNotNull = pseudos.Where(c => c != null).ToArray();

            string error = null;
            List<Player> players = new List<Player>(8);
            for (int i = 0; i < pseudosNotNull.Length; i++)
            {
                Player player = PlayerDal.Details(pseudosNotNull[i]);
                if (player != null && !players.Contains(player))
                    players.Add(player);
                else
                    error = $"Le pseudo {pseudosNotNull[i]} n'existe pas ou est déjà dans la partie";
            }
            if (error != null)
            {
                ViewBag.error = error;
                return View(pseudos);
            }

            ChrominoDal.CreateChrominos();
            int gameId = GameDal.AddGame().Id;
            GamePlayerDal.Add(gameId, players);
            GameCore gamecore = new GameCore(Ctx, gameId);
            gamecore.BeginGame(players.Count);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        public IActionResult ContinueGame(int id)
        {
            GetPlayerInfosFromSession();

            List<Player> players = GamePlayerDal.Players(id);
            if (players.Count == 1 && PlayerDal.IsBot(players[0].Id))
            {
                return RedirectToAction("PlayBot", "Game", new { id });
            }
            else
            {
                // TODO jeux "normal"
                return View();
            }
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
            GetPlayerInfosFromSession();

            GameCore gameCore = new GameCore(Ctx, gameId);
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chrominoId,
                XPosition = x,
                YPosition = y,
                Orientation = orientation,
            };
            PlayReturn playReturn = gameCore.Play(chrominoInGame, playerId);

            if (playReturn == PlayReturn.Ok)
                NextPlayerPlayIfBot(gameId, gameCore);
            else
                TempData["PlayReturn"] = playReturn;
            
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
            GetPlayerInfosFromSession();

            GameCore gameCore = new GameCore(Ctx, gameId);
            int playersNumber = GamePlayerDal.PlayersNumber(gameId);
            GamePlayer gamePlayer = GamePlayerDal.Details(gameId, playerId);
            if (playerId == PlayerId && (!gamePlayer.PreviouslyDraw || playersNumber == 1))
            {
                gameCore.DrawChromino(playerId);
            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        [HttpPost]
        public IActionResult PassTurn(int playerId, int gameId)
        {
            GetPlayerInfosFromSession();
            if (playerId == PlayerId)
            {
                GameCore gameCore = new GameCore(Ctx, gameId);
                gameCore.PassTurn(playerId);
                NextPlayerPlayIfBot(gameId, gameCore);
            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }


        public IActionResult Show(int id)
        {
            GetPlayerInfosFromSession();

            //TODO passer l'essentiel de l'algo dans GameCore

            List<Player> players = GamePlayerDal.Players(id);
            int playersNumber = players.Count;
            Player playerTurn = GamePlayerDal.PlayerTurn(id);

            if (players.Where(x => x.Id == PlayerId).FirstOrDefault() != null || playerTurn.Bot) // identified player in the game or bot play
            {
                int chrominosInGameNumber = GameChrominoDal.InGame(id);
                int chrominosInStackNumber = GameChrominoDal.InStack(id);

                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    if (player.Id != PlayerId)
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(id, player.Id));
                }

                Dictionary<string, Chromino> pseudos_lastChrominos = new Dictionary<string, Chromino>();
                foreach (var pseudo_chromino in pseudos_chrominos)
                {
                    if (pseudo_chromino.Value == 1)
                    {
                        pseudos_lastChrominos.Add(pseudo_chromino.Key, GameChrominoDal.FirstChromino(id, GamePlayerDal.PlayerId(id, pseudo_chromino.Key)));
                    }
                }


                List<Chromino> identifiedPlayerChrominos = new List<Chromino>();
                if (GamePlayerDal.IsAllBot(id)) // s'il n'y a que des bots en jeu, on regarde la partie et leur mains
                {
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, playerTurn.Id);
                }
                else
                {
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, PlayerId);
                }
                Game game = GameDal.Details(id);
                GameStatus gameStatus = game.Status;
                if (gameStatus == GameStatus.Finished)
                {
                    GamePlayerDal.SetViewFinished(id, PlayerId);
                    GamePlayerDal.SetWon(id, PlayerId, false);
                }
                else if (gameStatus == GameStatus.SingleFinished)
                {
                    GamePlayerDal.SetViewFinished(id, PlayerId);
                    GamePlayerDal.SetWon(id, PlayerId, false);
                }
                GamePlayer gamePlayerTurn = GamePlayerDal.Details(id, playerTurn.Id);
                List<Square> squares = SquareDal.List(id);
                List<int> botsId = PlayerDal.BotsId();
                List<ChrominoInGame> chrominosInGamePlayed = GameChrominoDal.ChrominosInGamePlayed(id);
                List<string> pseudoChrominosInGamePlayed = new List<string>();
                foreach (ChrominoInGame chrominoInGame in chrominosInGamePlayed)
                {
                    if (chrominoInGame.PlayerId != null)
                    {
                        pseudoChrominosInGamePlayed.Add(PlayerDal.Details((int)chrominoInGame.PlayerId).Pseudo);
                    }
                    else
                    {
                        pseudoChrominosInGamePlayed.Add("premier chromino");
                    }

                }
                GameVM gameViewModel = new GameVM(id, squares, gameStatus, chrominosInGameNumber, chrominosInStackNumber, pseudos_chrominos, identifiedPlayerChrominos, playerTurn, gamePlayerTurn, botsId, pseudos_lastChrominos, chrominosInGamePlayed, pseudoChrominosInGamePlayed);
                return View(gameViewModel);
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult PlayBot(int id, int botId)
        {
            GetPlayerInfosFromSession();
            GameCore gamecore = new GameCore(Ctx, id);
            gamecore.PlayBot(botId);
            return RedirectToAction("Show", "Game", new { id });
        }

        private void NextPlayerPlayIfBot(int gameId, GameCore gameCore)
        {
            int playerId = GamePlayerDal.PlayerTurn(gameId).Id;
            if (PlayerDal.IsBot(playerId))
            {
                while (gameCore.PlayBot(playerId) != PlayReturn.Ok) ;
            }
        }
    }
}
