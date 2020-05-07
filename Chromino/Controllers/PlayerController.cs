using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
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
        private SignInManager<Player> SignInManager { get; set; }

        public PlayerController(Context context, UserManager<Player> userManager, SignInManager<Player> signInManager) : base(context, userManager, null)
        {
            SignInManager = signInManager;
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
            await SignInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public JsonResult PlayersInfos(int gameId)
        {
            List<Object> playersInfos = new List<Object>();
            foreach (int id in GamePlayerDal.PlayersId(gameId))
            {
                int chrominosNumber = ChrominoInHandDal.ChrominosNumber(gameId, id);
                string[] lastChrominoColors = new string[] { "", "", "" };
                string name = id == PlayerId ? "Vous" : PlayerDal.Name(id);
                bool isBot = PlayerDal.IsBot(id);
                if (chrominosNumber == 1)
                {
                    Chromino c = ChrominoInHandDal.FirstChromino(gameId, id);
                    lastChrominoColors = new string[] { c.FirstColor.ToString(), c.SecondColor.ToString(), c.ThirdColor.ToString() };
                }
                playersInfos.Add(new { id, isBot, name, chrominosNumber, lastChrominoColors });
            }
            bool opponentsAllBots = GamePlayerDal.IsAllBots(gameId, PlayerId);
            return new JsonResult(new { playersInfos, opponentsAllBots } );
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
    }
}
