using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Controllers
{
    public class PlayerController : CommonController
    {
        protected ChrominoInHandLastDal ChrominoInHandLastDal { get; }
        private SignInManager<Player> SignInManager { get; set; }
        private RoleManager<IdentityRole<int>> RoleManager { get; set; }

        public PlayerController(Context context, UserManager<Player> userManager, SignInManager<Player> signInManager, RoleManager<IdentityRole<int>> roleManager) : base(context, userManager, null)
        {
            SignInManager = signInManager;
            RoleManager = roleManager;
            ChrominoInHandLastDal = new ChrominoInHandLastDal(Ctx);
        }

        /// <summary>
        /// Creation d'un joueur anonyme et connexion
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> NoAccountAsync()
        {
            string guestName = "invit";
            string guid = Guid.NewGuid().ToString("N");
            while (UserManager.FindByNameAsync(guestName + guid).Result != null)
                guid = Guid.NewGuid().ToString("N");
            Player user = new Player { UserName = guestName + guid };
            var result = await UserManager.CreateAsync(user);
            const string playerRoleGuest = "Guest";
            bool roleExist = await RoleManager.RoleExistsAsync(playerRoleGuest);
            if (!roleExist)
                await RoleManager.CreateAsync(new IdentityRole<int>(playerRoleGuest));
            await UserManager.AddToRoleAsync(user, playerRoleGuest);
            await SignInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
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

        [Authorize]
        public IActionResult ContactsIdsNames(List<int> ids)
        {
            object players = User.IsInRole("Admin") ? PlayerDal.HumansIdsNames(PlayerId) : ContactDal.ContactsId_Names(PlayerId);
            return new JsonResult(new { players });
        }
    }
}
