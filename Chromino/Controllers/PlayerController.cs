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
        public PlayerController(Context context, UserManager<Player> userManager) : base(context, userManager)
        {
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Login([Bind("Pseudo,Password")] Player player)
        //{
        //    Contract.Requires(player != null);
        //    if (ModelState.IsValid)
        //    {
        //        player.Password = player.Password.GetHash();
        //        Player found;
        //        if ((found = PlayerDal.GetPlayer(player)) != null)
        //        {
        //            HttpContext.Session.SetInt32(SessionKeyPlayerId, found.Id);
        //            HttpContext.Session.SetString(SessionKeyPlayerPseudo, found.Pseudo);
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            ViewBag.error = "Votre pseudo ou mot de passe est incorrect. Merci de réessayer";
        //            return View();
        //        }

        //    }
        //    else
        //    {
        //        ViewBag.error = "Votre pseudo ou mot de passe est incorrect. Merci de réessayer";
        //        return View();
        //    }
        //}

        [HttpPost]
        public IActionResult LoginGuest()
        {
            Player found = PlayerDal.Details("Invité");
            if (found != null)
            {
                HttpContext.Session.SetInt32(SessionKeyPlayerId, found.Id);
                HttpContext.Session.SetString(SessionKeyPlayerPseudo, found.UserName);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
