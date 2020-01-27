using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.ViewModel;
using System.Globalization;

namespace ChrominoGame.Controllers
{
    public class GameController : Controller
    {
        private readonly DefaultContext Ctx;
        private readonly GameDal GameDal;
        private readonly GameChrominoDal GameChrominoDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly SquareDal SquareDal;



        public GameController(DefaultContext context)
        {
            Ctx = context;
            GameDal = new GameDal(Ctx);
            GameChrominoDal = new GameChrominoDal(Ctx);
            ChrominoDal = new ChrominoDal(Ctx);
            PlayerDal = new PlayerDal(Ctx);
            GamePlayerDal = new GamePlayerDal(Ctx);
            SquareDal = new SquareDal(Ctx);
        }

        [HttpGet]
        public IActionResult StartNew()
        {
            return View(null);
        }


        [HttpPost]
        public IActionResult StartNew(string[] pseudos)
        {
            if (pseudos == null || pseudos.Length == 0)
            {
                return View();
            }

            string error = null;
            Player playerBot = PlayerDal.Bot();

            List<Player> players = new List<Player>(8);
            if (pseudos[0] != null && pseudos[0].ToUpperInvariant() != "BOT")
            {
                Player player = PlayerDal.Detail(pseudos[0]);
                if (player != null)
                    players.Add(player);
                else
                    error = "pseudo 1 doesn't exist";
            }
            else
            {
                players.Add(playerBot);
            }
            for (int i = 1; i < pseudos.Length; i++)
            {
                if (pseudos[i] != null)
                {
                    Player player = PlayerDal.Detail(pseudos[i]);
                    if (player != null)
                        players.Add(player);
                    else
                        error = $"pseudo {i} doesn't exist";
                }
            }

            if (error != null)
            {
                ViewBag.error = error;
                return View(pseudos);
            }

            ChrominoDal.CreateChrominos();
            int gameId = GameDal.AddGame().Id;
            GamePlayerDal.Add(gameId, players);
            GameChrominoDal.Add(gameId);
            GameCore gamecore = new GameCore(Ctx, gameId, players);
            gamecore.BeginGame();
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        public IActionResult ContinueGame(int id)
        {
            List<Player> players = GamePlayerDal.Players(id);
            if (players.Count == 1 && players[0].Pseudo == "bot")
            {
                return RedirectToAction("ContinueRandomGame", "Game", new { id });
            }
            else
            {
                // TODO jeux "normal"
                return View();
            }
        }

        public IActionResult ContinueRandomGame(int id)
        {
            Player bot = PlayerDal.Bot();
            List<Player> players = new List<Player>(1) { bot };
            GameCore gamecore = new GameCore(Ctx, id, players);
            gamecore.ContinueRandomGame();
            return RedirectToAction("Show", "Game", new { id });
        }

        [HttpPost]
        public IActionResult Play(int playerId, int gameId, int chrominoId, int x, int y, int xMin, int yMin, Orientation orientation)
        {
            Player player = PlayerDal.Detail(playerId);
            List<Player> players = GamePlayerDal.Players(gameId);
            GameCore gamecore = new GameCore(Ctx, gameId, players);

            //Coordinate coordinate = new Coordinate(x, y);
            GameChromino gameChromino = GameChrominoDal.Details(gameId, chrominoId);
            gameChromino.XPosition = x + xMin;
            gameChromino.YPosition = y + yMin;
            gameChromino.Orientation = orientation;
            gameChromino.PlayerId = playerId;

            bool move = gamecore.Play(gameChromino);
            if (!move)
            {
                // todo : position pas bonne. avertir joueur
                int a = 5;
            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }


        [HttpPost]
        public IActionResult PickChromino(int playerId, int gameId)
        {
            GameChromino gameChromino = GameChrominoDal.ChrominoFromStackToHandPlayer(gameId, playerId);
            if (gameChromino == null)
                GameDal.SetStatus(gameId, GameStatus.Finished);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }


        public IActionResult Show(int id)
        {
            int chrominosInGame = GameChrominoDal.StatusNumber(id, ChrominoStatus.InGame);
            int chrominosInStack = GameChrominoDal.StatusNumber(id, ChrominoStatus.InStack);

            List<Player> players = GamePlayerDal.Players(id);
            List<int> numberChrominosInHand = new List<int>(players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                numberChrominosInHand.Add(GameChrominoDal.PlayerNumberChrominos(id, players[i].Id));
            }

            List<Chromino> identifiedPlayerChrominos = new List<Chromino>();
            if (players.Count == 1 && players[0].Pseudo == "Bot")
            {
                identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, players[0].Id);
            }
            else if (players.Count == 1) // le joueur identifié est nécessairement le joueur courant
            {
                identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, players[0].Id);
            }
            else
            {
                throw new NotImplementedException();
            }

            Game game = GameDal.Details(id);
            GameStatus gameStatus = game.Status;
            bool autoPlay = game.AutoPlay;

            List<Square> squares = SquareDal.List(id);
            GameViewModel gameViewModel = new GameViewModel(id, squares, autoPlay, gameStatus, chrominosInGame, chrominosInStack, numberChrominosInHand, identifiedPlayerChrominos);

            //ViewData["ChrominosId"] = new SelectList(identifiedPlayerChrominos, "Id", "Id");
            return View(gameViewModel);
        }

        [HttpPost]
        public IActionResult AutoPlay(int gameId, bool autoPlay)
        {
            GameDal.SetAutoPlay(gameId, autoPlay);
            return RedirectToAction("ContinueRandomGame", "Game", new { id = gameId });
        }
    }
}
