using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace Chromino.Controllers
{
    public class GridController : Controller
    {
        private readonly DefaultContext Ctx;
        private readonly SquareDal GridDal;
        private readonly GameChrominoDal GameChrominoDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;
        public GridController(DefaultContext context)
        {
            Ctx = context;
            GridDal = new SquareDal(Ctx);
            GameChrominoDal = new GameChrominoDal(Ctx);
            GamePlayerDal = new GamePlayerDal(Ctx);
            GameDal = new GameDal(Ctx);
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

            Game game = GameDal.Details(id);
            GameStatus gameStatus = game.Status;
            bool autoPlay = game.AutoPlay;

            List<Square> grids = GridDal.List(id);
            GameViewModel gameViewModel = new GameViewModel(id, grids, autoPlay, gameStatus, chrominosInGame, chrominosInStack, numberChrominosInHand);

            return View(gameViewModel);
        }

        [HttpPost]
        public IActionResult AutoPlay(int gameId, bool autoPlay)
        {
            if (autoPlay)
                GameDal.SetAutoPlay(gameId, autoPlay);

            return RedirectToAction("Show", new { id = gameId });
        }
    }
}