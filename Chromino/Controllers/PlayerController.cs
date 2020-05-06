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
        private SignInManager<Player> SignInManager { get; set; }

        public PlayerController(Context context, UserManager<Player> userManager, SignInManager<Player> signInManager) : base(context, userManager, null)
        {
            SignInManager = signInManager;
            ChrominoInHandLastDal = new ChrominoInHandLastDal(Ctx);
        }

        public JsonResult IdsChrominosNumber(int gameId)
        {
            List<Object> playersWithInfos = new List<Object>();
            foreach (int playerId in GamePlayerDal.PlayersId(gameId))
            {
                int chrominosNumber = ChrominoInHandDal.ChrominosNumber(gameId, playerId);
                string[] lastChrominoColors = new string[] { "", "", "" };
                if (chrominosNumber == 1)
                {
                    Chromino c = ChrominoInHandDal.FirstChromino(gameId, playerId);
                    lastChrominoColors = new string[] { c.FirstColor.ToString(), c.SecondColor.ToString(), c.ThirdColor.ToString() };
                }
                string name = playerId == PlayerId ? "Vous" : PlayerDal.Name(playerId);
                bool isBot = PlayerDal.IsBot(playerId);
                playersWithInfos.Add(new { id = playerId, isBot, name, chrominosNumber, lastChrominoColors });
            }
            return new JsonResult(playersWithInfos);
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
