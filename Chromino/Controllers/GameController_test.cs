﻿#if DEBUG

using Data.Core;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
    [Authorize]
    public partial class GameController : CommonController
    {
        [HttpGet]
        public IActionResult NewTest()
        {
            if (PlayerPseudo != "Thomas")
                return RedirectToAction("Index", "Home");

            Player player1 = PlayerDal.Details(PlayerDal.BotsId()[0]);
            Player player2 = PlayerDal.Details(PlayerId);
            List<Player> players = new List<Player>(2);
            players.Add(player1);
            players.Add(player2);
            CreateGameTestDebug(ref players, out int gameId);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        [HttpGet]
        public async Task<IActionResult> NewTest2Async()
        {
            if (PlayerPseudo != "Thomas")
                return RedirectToAction("Index", "Home");

            Player player1 = PlayerDal.Details(PlayerId);
            Player player2 = PlayerDal.Details(PlayerDal.BotsId()[0]);

            List<Player> players = new List<Player>(2);
            players.Add(player1);
            players.Add(player2);
            CreateGameTestDebug(ref players, out int gameId, false);

            GameBI gamecore = new GameBI(Ctx, Env, gameId);

            gamecore.TestAsync(gameId);

            await Task.Delay(5000);

            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        [HttpGet]
        public async Task<IActionResult> NewTestCompute()
        {
            if (PlayerPseudo != "Thomas")
                return RedirectToAction("Index", "Home");

            Player player1 = PlayerDal.Details(PlayerId);
            Player player2 = PlayerDal.Details(PlayerDal.BotsId()[0]);

            List<Player> players = new List<Player>(2);
            players.Add(player1);
            players.Add(player2);
            CreateGameTestDebug(ref players, out int gameId, false, true);

            GameBI gamecore = new GameBI(Ctx, Env, gameId);

            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        private void CreateGameTestDebug(ref List<Player> players, out int gameId, bool copyGame = true, bool firstChrominoPlay = false)
        {
            gameId = GameDal.AddTestDebug().Id;

            if (copyGame)
            {
                int gameToCopy = 5080;
                SquareDal squareDal = new SquareDal(Ctx);
                var squares = squareDal.List(gameToCopy);
                squareDal.AddTestDebug(squares, gameId);
            }

            GamePlayerDal.Add(gameId, players);
            GameBI gamecore = new GameBI(Ctx, Env, gameId);
            gamecore.BeginGameTest(firstChrominoPlay);
        }
    }
}

#endif