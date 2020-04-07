using Controllers;
using Data;
using Data.Core;
using Data.DAL;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tool;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class GamesController : CommonController
    {
        public GamesController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        /// <summary>
        /// Page d'accueil des parties
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            //TempData["GamesWithNotReadMessages"] = MakePicturesGameVM(GamePlayerDal.GamesWithNotReadMessages(PlayerId));
            return View();
        }

        /// <summary>
        /// Page des parties entre amis
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Player")]
        public IActionResult AgainstFriends()
        {
            return View(MakePicturesGameVM(GamePlayerDal.MultiGamesAgainstAtLeast1HumanToPlay(PlayerId), true));
        }

        /// <summary>
        /// Page des parties à jouer contre uniquement des bots 
        /// </summary>
        /// <returns></returns>
        public IActionResult AgainstBots()
        {
            return View(MakePicturesGameVM(GamePlayerDal.GamesAgainstBotsOnly(PlayerId), true));
        }

        /// <summary>
        /// Page des parties d'entrainement en cours
        /// </summary>
        /// <returns></returns>
        public IActionResult SingleGame()
        {
            List<PictureGameVM> picturesGameVM = MakePicturesGameVM(GamePlayerDal.SingleGamesInProgress(PlayerId));
            if (picturesGameVM.Count == 0)
                return RedirectToAction("NewSingle", "Game");
            else
                return View(picturesGameVM);
        }

        /// <summary>
        /// Page des parties en cours (tour d'un adversaire)
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Player")]
        public IActionResult InProgress()
        {
            return View(MakePicturesGameVM(GamePlayerDal.GamesWaitTurn(PlayerId), true));
        }

        /// <summary>
        /// Page des parties solo terminées
        /// </summary>
        /// <returns></returns>
        public IActionResult SingleFinished()
        {
            return View(MakePicturesGameVM(GamePlayerDal.SingleGamesFinished(PlayerId)));
        }
    }
}