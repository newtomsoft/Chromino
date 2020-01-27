using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChrominoGame.Models;
using Data;
using Tool;
using Data.Core;
using Data.DAL;
using Data.Models;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Http;

namespace Controllers
{
    public class HomeController : CommonController
    {
        public HomeController(DefaultContext ctx) : base(ctx)
        {
        }

        public IActionResult Index()
        {
            //Jeu jeu = new Jeu(ctx);

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Pseudo,Password")] Player player)
        {
            Contract.Requires(player != null);
            if (ModelState.IsValid)
            {
                player.Password = player.Password.GetHash();
                Player found;
                if ((found = PlayerDal.GetPlayer(player)) != null)
                {
                    HttpContext.Session.SetInt32(SessionKeyPlayerId, found.Id);
                    HttpContext.Session.SetString(SessionKeyPlayerPseudo, found.Pseudo);
                }
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        public IActionResult Privacy()
        {
            GetPlayerInfosFromSession();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
