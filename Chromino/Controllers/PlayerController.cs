using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Tool;


namespace Controllers
{
    public class PlayerController : CommonController
    {
        protected ChrominoInHandLastDal ChrominoInHandLastDal { get; }
        protected GoodPositionDal GoodPositionDal { get; }
        private SignInManager<Player> SignInManager { get; set; }

        public PlayerController(Context context, UserManager<Player> userManager, SignInManager<Player> signInManager) : base(context, userManager, null)
        {
            SignInManager = signInManager;
            ChrominoInHandLastDal = new ChrominoInHandLastDal(Ctx);
            GoodPositionDal = new GoodPositionDal(Ctx);
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

        public IActionResult Delete()
        {
            var gamesIdToDelete = GamePlayerDal.GamesId(PlayerId);
            ChrominoInGameDal.Delete(gamesIdToDelete);
            ChrominoInHandDal.Delete(gamesIdToDelete);
            ChrominoInHandLastDal.Delete(gamesIdToDelete);
            GamePlayerDal.Delete(gamesIdToDelete);
            GoodPositionDal.Delete(gamesIdToDelete);
            SquareDal.Delete(gamesIdToDelete);
            GameDal.Delete(gamesIdToDelete);
            PlayerDal.Delete(PlayerId);
            SignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
