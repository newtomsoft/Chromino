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
            string info = "show";
            int id = GamePlayerDal.FirstIdMultiGameToPlay(PlayerId);
            return id == 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Show", new { id, info });
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
            List<string> errors = new List<string>();
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
                {
                    players.Add(player);
                    ContactDal.Add(PlayerId, player.Id);
                    ContactDal.Add(player.Id, PlayerId);
                }
            }
            if (botsNumber + players.Count > MaxPlayers)
                errors.Add($"Le nombre de joueurs est supérieur à {MaxPlayers}. Il ne faut pas chercher à dépasser les bornes des limites ! ;-)");
            else if (botsNumber < 0)
                errors.Add($"Le nombre de bots est inférieur à 0. Bien tenté mais ça ne passe pas ici ! 😉");

            if (errors.Count != 0)
            {
                TempData["Errors"] = errors;
                TempData["PlayersNumber"] = pseudosNotNull.Length;
                TempData["Players"] = pseudos;
                return RedirectToAction("AgainstFriends", "Games", "newgame");
            }
            for (int i = 1; i < botsNumber + 1; i++)
                players.Add(PlayerDal.Details("bot" + i));
            CreateGame(players, out int id);
            return RedirectToAction("Show", new { id });
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
            CreateGame(players, out int id);
            return RedirectToAction("Show", new { id });
        }

        /// <summary>
        /// page de création de partie solo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult NewSingle()
        {
            List<Player> players = new List<Player> { PlayerDal.Details(PlayerId) };
            CreateGame(players, out int id);
            return RedirectToAction("Show", new { id });
        }

        /// <summary>
        /// création d'une partie "revanche"
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Rematch(string[] playersName)
        {
            List<Player> players = new List<Player>();
            foreach (string name in playersName)
                players.Add(PlayerDal.Details(name));
            CreateGame(players, out int id);
            return RedirectToAction("Show", new { id });
        }

        /// <summary>
        /// affiche une partie 
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <returns></returns>
        public async Task<IActionResult> ShowAsync(int id)
        {
            if (!GameDal.IsExist(id) || !GamePlayerDal.IsPlayerIn(id, PlayerId) && !IsAdmin)
                return RedirectToAction("GameNotFound");
            return View();
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
            PlayReturn playReturn = IsTurn(gameId, PlayerId);
            if (playReturn != PlayReturn.Ok)
                return new JsonResult(new { errorReturn = playReturn.ToString() });
            if (ChrominoInHandDal.ChrominosNumber(gameId, PlayerId) == 1 && ChrominoDal.IsCameleon(chrominoId))
                return new JsonResult(new { errorReturn = PlayReturn.LastChrominoIsCameleon.ToString() });

            ChrominoInGame chrominoInGame = new ChrominoInGame() { GameId = gameId, PlayerId = PlayerId, ChrominoId = chrominoId, XPosition = x, YPosition = y, Orientation = orientation, Flip = flip };
            playReturn = new PlayerBI(Ctx, Env, gameId, PlayerId).Play(chrominoInGame);
            if (playReturn.IsError())
                return new JsonResult(new { errorReturn = playReturn.ToString() });
            else
            {
                List<string> lastChrominoColors = ColorsLastChromino(gameId, PlayerId);
                List<string> colors = ColorsPlayedChromino(chrominoId);
                bool finish = GameDal.IsFinished(gameId);
                return new JsonResult(new { colors, lastChrominoColors, finish });
            }
        }

        /// <summary>
        /// fait jouer un bot
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <param name="botId">Id du bot</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult PlayBot(int id, int botId)
        {
            PlayReturn playReturn;
            ChrominoInGame chrominoInGame;
            BotBI botBI = new BotBI(Ctx, Env, id, botId);
            bool draw = false;
            do
            {
                playReturn = botBI.PlayBot(out chrominoInGame);
                if (playReturn == PlayReturn.DrawChromino)
                    draw = true;
            }
            while (playReturn.IsError() || playReturn == PlayReturn.DrawChromino);

            List<string> lastChrominoColors = ColorsLastChromino(id, botId);
            bool finish = GameDal.GetStatus(id).IsFinished();
            if (chrominoInGame != null)
            {
                bool skip = false;
                int x = chrominoInGame.XPosition; int y = chrominoInGame.YPosition; Orientation orientation = chrominoInGame.Orientation; bool flip = chrominoInGame.Flip;
                List<string> colors = ColorsPlayedChromino((int)chrominoInGame.ChrominoId);
                return new JsonResult(new { skip, draw, x, y, orientation, flip, colors, lastChrominoColors, finish });
            }
            else
            {
                bool skip = true;
                return new JsonResult(new { skip, draw, finish });
            }
        }

        /// <summary>
        /// Pioche un chromino
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DrawChromino(int gameId)
        {
            if (GamePlayerDal.PlayerTurn(gameId).Id != PlayerId)
                return new JsonResult(new { errorReturn = PlayReturn.NotPlayerTurn.ToString() });
            if (GamePlayerDal.Details(gameId, PlayerId).PreviouslyDraw && GamePlayerDal.PlayersNumber(gameId) != 1)
                return new JsonResult(new { errorReturn = PlayReturn.CantDraw2TimesInARow.ToString() });
            if (ChrominoInGameDal.InStack(gameId) == 0)
                return new JsonResult(new { errorReturn = PlayReturn.NoMoreChrominosInStack.ToString() });

            int id = new PlayerBI(Ctx, Env, gameId, PlayerId).TryDrawChromino(out _);
            List<string> colors = ColorsPlayedChromino(id);
            return new JsonResult(new { id, colors });
        }

        /// <summary>
        /// Passe le tour du joueur
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult SkipTurn(int gameId)
        {
            if (GamePlayerDal.PlayerTurn(gameId).Id != PlayerId)
                return new JsonResult(new { errorReturn = PlayReturn.NotPlayerTurn.ToString() });

            int id = new PlayerBI(Ctx, Env, gameId, PlayerId).SkipTurn();
            bool finish = GameDal.GetStatus(gameId).IsFinished();
            return new JsonResult(new { id, finish });
        }

        /// <summary>
        /// Affiche les positions possible pour les chrominos de la main du joueur
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Help(int gameId)
        {
            bool status = PlayerDal.DecreaseHelp(PlayerId);
            HashSet<int> indexes = new HashSet<int>();
            if (status)
            {
                int xMin = SquareDal.XMin(gameId) - 2; // -2 : marges
                int xMax = SquareDal.XMax(gameId) + 2;
                int yMin = SquareDal.YMin(gameId) - 2;
                int columnsNumber = xMax - xMin + 1;
                List<GoodPosition> goodPositions = GoodPositionDal.RootListByPriority(gameId, PlayerId);
                var possiblesChrominosVM = new List<PossiblesChrominoVM>();
                foreach (GoodPosition goodPosition in goodPositions)
                    possiblesChrominosVM.Add(new PossiblesChrominoVM(goodPosition, xMin, yMin));
                for (int iChromino = 0; iChromino < possiblesChrominosVM.Count; iChromino++)
                    for (int i = 0; i < 3; i++)
                        indexes.Add(possiblesChrominosVM[iChromino].IndexesX[i] + possiblesChrominosVM[iChromino].IndexesY[i] * columnsNumber);
                if (indexes.Count == 0)
                    PlayerDal.IncreaseHelp(PlayerId);
            }
            return new JsonResult(new { indexes });
        }

        /// <summary>
        /// Page de partie non trouvée ou non autorisée
        /// </summary>
        /// <returns></returns>
        public IActionResult GameNotFound()
        {
            return View();
        }

        public async Task<IActionResult> Infos(int gameId)
        {
            const int horizontal = (int)Orientation.Horizontal;
            const int vertical = (int)Orientation.Vertical;
            Player thisPlayer = PlayerDal.Details(PlayerId);
            bool opponentsAllBots = GamePlayerDal.IsAllBots(gameId, PlayerId);
            List<object> playersInfos = new List<object>();
            foreach (int id in GamePlayerDal.PlayersId(gameId))
            {
                int chrominosNumber = ChrominoInHandDal.ChrominosNumber(gameId, id);
                string[] lastChrominoColors = new string[] { "", "", "" };
                Player player = PlayerDal.Details(id);
                string name = player.UserName;
                bool isBot = player.Bot;
                if (chrominosNumber == 1)
                {
                    Chromino c = ChrominoInHandDal.FirstChromino(gameId, id);
                    lastChrominoColors = new string[] { c.FirstColor.ToString(), c.SecondColor.ToString(), c.ThirdColor.ToString() };
                }
                playersInfos.Add(new { id, isBot, name, chrominosNumber, lastChrominoColors });
            }
            List<object> colors = new List<object>();
            foreach (ColorCh color in (ColorCh[])Enum.GetValues(typeof(ColorCh)))
            {
                int id = (int)color;
                string name = color.ToString();
                colors.Add(new { id, name });
            }
            GameVM gameVM = new GameBI(Ctx, Env, gameId).GameVM(PlayerId, IsAdmin);
            return new JsonResult(new { gameVM, opponentsAllBots, playersInfos, horizontal, vertical, colors });
        }

        /// <summary>
        /// Cloture de la partie
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult End(int gameId)
        {
            if (GameDal.IsFinished(gameId) && !GamePlayerDal.IsViewFinished(gameId, PlayerId))
            {
                PlayerBI playerBI = new PlayerBI(Ctx, Env, gameId, PlayerId);
                if (GamePlayerDal.GetWon(gameId, PlayerId) != true)
                {
                    playerBI.LooseGame(PlayerId);
                    if (GamePlayerDal.IsAllLoosersViewFinished(gameId))
                        return new JsonResult(new { askRematch = true });
                    return new JsonResult(new { askRematch = false });
                }
                if (GamePlayerDal.WinnersId(gameId).Count > 1) // draw game
                {
                    playerBI.WinGame(PlayerId, true);
                    return new JsonResult(null);
                }
                // only 1 winner
                playerBI.WinGame(PlayerId);
                return new JsonResult(null);
            }
            return new JsonResult(new { dejavu = true });
        }

        /// <summary>
        /// donne les infos du joueur venant de poser un chromino 
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <param name="playerId">id du joueur qui vient de jouer</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult InfosAfterPlaying(int gameId, int playerId)
        {
            List<string> lastChrominoColors = ColorsLastChromino(gameId, playerId);
            bool finish = GameDal.GetStatus(gameId).IsFinished();
            return new JsonResult(new { lastChrominoColors, finish });
        }

        private void CreateGame(List<Player> players, out int gameId)
        {
            List<Player> randomPlayers = players.RandomSort();
            ChrominoDal.CreateChrominos();
            gameId = GameDal.Add().Id;
            GamePlayerDal.Add(gameId, randomPlayers);
            new GameBI(Ctx, Env, gameId).BeginGame(randomPlayers.Count);
        }

        private List<string> ColorsLastChromino(int gameId, int playerId)
        {
            List<string> lastChrominoColors = new List<string>(3);
            if (ChrominoInHandDal.ChrominosNumber(gameId, playerId) == 1)
            {
                Chromino lastChromino = ChrominoInHandDal.FirstChromino(gameId, playerId);
                lastChrominoColors.Add(lastChromino.FirstColor.ToString());
                lastChrominoColors.Add(lastChromino.SecondColor.ToString());
                lastChrominoColors.Add(lastChromino.ThirdColor.ToString());
            }
            return lastChrominoColors;
        }

        private List<string> ColorsPlayedChromino(int chrominoId)
        {
            Chromino chromino = ChrominoDal.Details(chrominoId);
            if (chromino != null)
                return new List<string> { chromino.FirstColor.ToString(), chromino.SecondColor.ToString(), chromino.ThirdColor.ToString() };
            else
                return new List<string>(0);
        }

        private PlayReturn IsTurn(int gameId, int playerId)
        {
            if (GamePlayerDal.PlayerTurn(gameId).Id != playerId)
                return PlayReturn.NotPlayerTurn;
            else if (GameDal.IsFinished(gameId))
                return PlayReturn.ErrorGameFinish;
            else
                return PlayReturn.Ok;
        }
    }
}
