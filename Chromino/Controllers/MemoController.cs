using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Controllers;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class MemoController : CommonController
    {
        public MemoController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        [HttpPost]
        public IActionResult Add(int gameId, int playerId, string memo)
        {
            if (playerId == PlayerId)
            {
                GamePlayerDal.ChangeMemo(gameId, playerId, memo);
                return RedirectToAction("Show", "Game", new { id = gameId });
            }
            else
            {
                return RedirectToAction("NotFound");
            }
        }
    }
}