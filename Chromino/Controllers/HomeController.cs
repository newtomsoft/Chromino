using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Chromino.Models;
using Data;
using Data.Core;
using Data.DAL;

namespace Chromino.Controllers
{
    public class HomeController : Controller
    {
        private readonly ChrominoDal ChrominoDAL;

        public HomeController(DefaultContext ctx)
        {
            ChrominoDAL = new ChrominoDal(ctx);

        }

        public IActionResult Index()
        {
            //Jeu jeu = new Jeu(ctx);






            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
