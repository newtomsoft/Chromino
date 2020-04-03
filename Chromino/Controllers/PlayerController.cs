using Data;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Tool;

namespace Controllers
{
    public class PlayerController : CommonController
    {
        public PlayerController(Context context, UserManager<Player> userManager) : base(context, userManager, null)
        {
        }

        [HttpPost]
        public IActionResult DisableTips(int gameId, int playerId, string dontShowTips)
        {
            if (playerId == PlayerId && dontShowTips == "on")
            {
                PlayerDal.DisableTips(playerId);
            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }
    }
}
