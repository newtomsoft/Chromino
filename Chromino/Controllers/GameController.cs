using ChrominoBI;
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
using System.Threading.Tasks;
using Tool;

namespace Controllers
{
    [Authorize]
    public partial class GameController : CommonController
    {
        public GameController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        /// <summary>
        /// affiche à l'écran la prochaine partie à jouer
        /// </summary>
        /// <returns></returns>
        public IActionResult ShowNextToPlay()
        {
            TempData["ByShowNextToPlay"] = true;
            int gameId = GamePlayerDal.FirstIdMultiGameToPlay(PlayerId);
            return gameId == 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// page de création de partie
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult New()
        {
            return View(null);
        }

        /// <summary>
        /// retour du formulaire de création de partie
        /// </summary>
        /// <param name="pseudos">pseudos des joueurs de la partie à créer</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult New(string[] pseudos, int botsNumber)
        {
            const int MaxPlayers = 8;
            if (pseudos == null || pseudos.Length == 0)
                return View();

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
                else if (pseudosNotNull[i].Contains("bot", new StringComparison()))
                    errors.Add($"Le joueur {pseudosNotNull[i]} est un bot et ne peut pas être ajouté ici");
                else if (players.Contains(player))
                    errors.Add($"Le joueur {pseudosNotNull[i]} est ajouté plusieurs fois dans la partie");
                else
                    players.Add(player);
            }

            if (botsNumber + players.Count > MaxPlayers)
                errors.Add($"Le nombre de joueurs est supérieur à {MaxPlayers}. Il ne faut pas chercher à dépasser les bornes des limites ! ;-)");
            else if (botsNumber < 0)
                errors.Add($"Le nombre de bots est inférieur à 0. Bien tenté mais ça ne passe pas ici ! ;-)");

            if (errors.Count != 0)
            {
                TempData["Errors"] = errors;
                TempData["PlayersNumber"] = pseudosNotNull.Length;
                TempData["Players"] = pseudos;
                return RedirectToAction("AgainstFriends", "Games", "newgame");
            }
            for (int i = 1; i < botsNumber + 1; i++)
                players.Add(PlayerDal.Details("bot" + i));
            CreateGame(players, out int gameId);
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// retour du formulaire de création de partie contre bots
        /// </summary>
        /// <param name="botsNumber">nombre d'adversaires bots</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult NewAgainstBots(int botsNumber)
        {
            List<Player> players = new List<Player>(botsNumber + 1) { PlayerDal.Details(PlayerId) };
            for (int iBot = 1; iBot <= botsNumber; iBot++)
                players.Add(PlayerDal.Details("bot" + iBot));
            CreateGame(players, out int gameId);
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// page de création de partie solo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult NewSingle()
        {
            List<Player> players = new List<Player> { PlayerDal.Details(PlayerId) };
            CreateGame(players, out int gameId);
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// affiche une partie 
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <returns></returns>
        public async Task<IActionResult> ShowAsync(int id)
        {
            if (id == 0)
                return RedirectToAction("GameNotFound");

            ChrominoInHandDal.PlayersIdWithMinChrominos(id);



            Player player = PlayerDal.Details(PlayerId);
            bool isAdmin = await UserManager.IsInRoleAsync(player, "Admin");

            bool showPossiblesPositions = false;
            if (TempData["Help"] != null)
                showPossiblesPositions = true;

            GameVM gameVM = new GameBI(Ctx, Env, id).GameVM(PlayerId, isAdmin, showPossiblesPositions);
            if (gameVM != null)
            {
                if (GamePlayerDal.PlayerTurn(id).Bot)
                    ViewData["ShowBotPlayingInfoPopup"] = true;
                return View(gameVM);
            }
            else
            {
                return RedirectToAction("GameNotFound");
            }
        }

        /// <summary>
        /// tente de jouer le chromino à l'emplacement choisit par le joueur
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <param name="chrominoId">id du chromino</param>
        /// <param name="x">abscisse (0 étant le Caméléon du premier chromino du jeu)</param>
        /// <param name="y">ordonnée (0 étant le Caméléon du premier chromino du jeu)</param>
        /// <param name="orientation">vertical, horizontal</param>
        /// <param name="flip">true si le chromino est tourné de 180°</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Play(int gameId, int chrominoId, int x, int y, Orientation orientation, bool flip)
        {
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                PlayerId = PlayerId,
                ChrominoId = chrominoId,
                XPosition = x,
                YPosition = y,
                Orientation = orientation,
                Flip = flip,
            };

            PlayReturn playReturn = new PlayerBI(Ctx, Env, gameId, PlayerId).Play(chrominoInGame);
            if (playReturn.IsError())
                TempData["PlayReturn"] = playReturn; //todo voir si ajax doit appeler NextPlayerPlayIfBot
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// fait jouer un bot
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <param name="botId">Id du bot</param>
        /// <returns></returns>
        public IActionResult PlayBot(int id, int botId)
        {
            BotBI botBI = new BotBI(Ctx, Env, id, botId);
            PlayReturn playreturn;
            do playreturn = botBI.PlayBot();
            while (playreturn.IsError() || playreturn == PlayReturn.DrawChromino);
            return RedirectToAction("Show", new { id });
        }

        /// <summary>
        /// Pioche un chromino
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DrawChromino(int gameId)
        {
            int playersNumber = GamePlayerDal.PlayersNumber(gameId);
            GamePlayer gamePlayer = GamePlayerDal.Details(gameId, PlayerId);
            if ((!gamePlayer.PreviouslyDraw || playersNumber == 1) && ChrominoInGameDal.InStack(gameId) > 0)
                new PlayerBI(Ctx, Env, gameId, PlayerId).TryDrawChromino(out _);

            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// Passe le tour du joueur
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SkipTurn(int gameId)
        {
            new PlayerBI(Ctx, Env, gameId, PlayerId).SkipTurn();
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// Affiche les positions possible pour les chrominos de la main du joueur
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Help(int gameId)
        {
            if (PlayerDal.DecreaseHelp(PlayerId))
                TempData["Help"] = true;
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// Page de partie non trouvée ou non autorisée
        /// </summary>
        /// <returns></returns>
        public IActionResult GameNotFound()
        {
            return View();
        }

        private void CreateGame(List<Player> players, out int gameId)
        {
            List<Player> randomPlayers = players.RandomSort();
            ChrominoDal.CreateChrominos();
            gameId = GameDal.Add().Id;
            GamePlayerDal.Add(gameId, randomPlayers);
            new GameBI(Ctx, Env, gameId).BeginGame(randomPlayers.Count);
        }
    }
}
