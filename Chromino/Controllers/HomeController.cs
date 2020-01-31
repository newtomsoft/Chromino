using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChrominoGame.Models;
using Data;
using Tool;
using Data.Core;
using Data.DAL;
using Data.Models;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Data.ViewModel;

namespace Controllers
{
    public class HomeController : CommonController
    {
        public HomeController(DefaultContext ctx) : base(ctx)
        {
        }

        public IActionResult Index()
        {
            GetPlayerInfosFromSession();
            if (PlayerId == 0)
            {
                return RedirectToAction("Login", "Player");
            }
            else
            {
                List<GameForListVM> gamesToPlayForListVM = new List<GameForListVM>();
                foreach (Game game in GamePlayerDal.MultiGamesToPlay(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    gamesToPlayForListVM.Add(new GameForListVM(game, pseudos_chrominos, playerPseudoTurn));
                }
                ViewData["GamesToPlay"] = new SelectList(gamesToPlayForListVM, "GameId", "Infos");


                List<GameForListVM> otherGamesForListVM = new List<GameForListVM>();
                foreach (Game game in GamePlayerDal.Games(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    otherGamesForListVM.Add(new GameForListVM(game, pseudos_chrominos, playerPseudoTurn));
                }
                ViewData["Games"] = new SelectList(otherGamesForListVM, "GameId", "Infos");

                //List<Game> gamesInProgress = GamePlayerDal.GamesInProgress(PlayerId);
                //ViewData["GamesInProgress"] = new SelectList(gamesInProgress.OrderByDescending(x => x.CreateDate), "Id", "CreateDate");
                //ViewData["GamesToPlay"] = new SelectList(gamesToPlay.OrderByDescending(x => x.CreateDate), "Id", "CreateDate");
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
