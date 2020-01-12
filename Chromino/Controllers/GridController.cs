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
        private readonly SquareDAL GridDAL;
        private readonly GameChrominoDal ChrominoGameDAL;
        private readonly GamePlayerDal GamePlayerDAL;
        private readonly GameDal GameDAL;
        public GridController(DefaultContext context)
        {
            Ctx = context;
            GridDAL = new SquareDAL(Ctx);
            ChrominoGameDAL = new GameChrominoDal(Ctx);
            GamePlayerDAL = new GamePlayerDal(Ctx);
            GameDAL = new GameDal(Ctx);
        }
        public IActionResult Show(int id)
        {
            int chrominosInGame = ChrominoGameDAL.InGame(id);
            int chrominosInStack = ChrominoGameDAL.InStack(id);

            List<Player> players = GamePlayerDAL.Players(id);
            List<int> numberChrominosInHand = new List<int>(players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                numberChrominosInHand.Add(ChrominoGameDAL.PlayerNumberChrominos(id, players[i].Id));
            }

            GameStatus gameStatus = GameDAL.Status(id);

            List<Square> grids = GridDAL.List(id);
            GameViewModel gameViewModel = new GameViewModel(grids, gameStatus, chrominosInGame, chrominosInStack, numberChrominosInHand);
            ViewBag.GameId = id;

            return View(gameViewModel);
        }
    }
}