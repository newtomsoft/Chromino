using Controllers;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class StatisticsController : CommonController
    {
        public StatisticsController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        public IActionResult Index(int gameId)
        {
            return View();
        }

        public IActionResult Won(int gameId)
        {
            return View(MakePicturesGameVM(GamePlayerDal.GamesWon(PlayerId)));
        }

        public IActionResult Lost(int gameId)
        {
            return View(MakePicturesGameVM(GamePlayerDal.GamesLost(PlayerId)));
        }
    }
}