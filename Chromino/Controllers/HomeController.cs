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
                List<Game> gamesInProgress = GamePlayerDal.GamesInProgress(PlayerId);
                List<Game> gamesToPlay = GamePlayerDal.GamesToPlay(PlayerId);

                ViewData["Games"] = new SelectList(GamePlayerDal.Games(PlayerId), "Id", "CreateDate", null, "Status");
                ViewData["GamesInProgress"] = new SelectList(gamesInProgress.OrderByDescending(x => x.CreateDate), "Id", "CreateDate");
                ViewData["GamesToPlay"] = new SelectList(gamesToPlay.OrderByDescending(x => x.CreateDate), "Id", "CreateDate");
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
