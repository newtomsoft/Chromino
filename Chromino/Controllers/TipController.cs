using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Tool;


namespace Controllers
{
    public class TipController : CommonController
    {
        protected TipDal TipDal { get; }
        public TipController(Context context, UserManager<Player> userManager, SignInManager<Player> signInManager) : base(context, userManager, null)
        {
            TipDal = new TipDal(Ctx);
        }

        [HttpPost]
        public IActionResult Off(int gameId, int tipId)
        {
            TipDal.SetOff(PlayerId, tipId);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }
    }
}
