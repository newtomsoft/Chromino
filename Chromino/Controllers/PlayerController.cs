using Data;
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
    public class PlayerController : CommonController
    {
        private SignInManager<Player> SignInManager { get; set; }
        public PlayerController(Context context, UserManager<Player> userManager, SignInManager<Player> signInManager) : base(context, userManager, null)
        {
            SignInManager = signInManager;
        }

        [HttpPost]
        public IActionResult DisableTips(int gameId, string dontShowTips)
        {
            PlayerDal.DisableTips(PlayerId);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        public async Task<IActionResult> NoAccountAsync()
        {
            string guestName = "invit";
            string guid = Guid.NewGuid().ToString("N");
            while (UserManager.FindByNameAsync(guestName + guid).Result != null)
                guid = Guid.NewGuid().ToString("N");

            Player user = new Player { UserName = guestName + guid };
            var result = await UserManager.CreateAsync(user);
            await SignInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
    }
}
