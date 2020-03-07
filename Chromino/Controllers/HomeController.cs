﻿using ChrominoGame.Models;
using Data;
using Data.DAL;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Controllers
{
    [Authorize]
    public class HomeController : CommonController
    {
        public HomeController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        /// <summary>
        /// Page des parties à jouer
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            GetPlayerInfos();
            ViewData["ListGamesToPlay"] = MakePicturesGameVM(GamePlayerDal.MultiGamesToPlay(PlayerId));
            ViewData["ListSingleGames"] = MakePicturesGameVM(GamePlayerDal.SingleGamesInProgress(PlayerId));
            return View();
        }
   
        /// <summary>
        /// Page des parties en cours (tour d'un adversaire)
        /// </summary>
        /// <returns></returns>
        public IActionResult GamesInProgress()
        {
            GetPlayerInfos();
            ViewData["ListGamesWaitTurn"] = MakePicturesGameVM(GamePlayerDal.GamesWaitTurn(PlayerId));
            return View();
        }

        /// <summary>
        /// Page des parties terminées
        /// </summary>
        /// <returns></returns>
        public IActionResult GamesFinished()
        {
            GetPlayerInfos();
            ViewData["ListGamesWon"] = MakePicturesGameVM(GamePlayerDal.GamesWon(PlayerId));
            ViewData["ListGamesLost"] = MakePicturesGameVM(GamePlayerDal.GamesLost(PlayerId));
            ViewData["ListSingleGamesFinished"] = MakePicturesGameVM(GamePlayerDal.SingleGamesFinished(PlayerId));
            return View();
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Rgpd()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Rules()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// fabrique la liste de PictureGameVM pour la vue
        /// </summary>
        /// <param name="games">liste des jeux</param>
        /// <returns></returns>
        private List<PictureGameVM> MakePicturesGameVM(List<Game> games)
        {
            List<PictureGameVM> listPictureGameVM = new List<PictureGameVM>();
            foreach (Game game in games)
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    int chrominosNumber = GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id);
                    if (chrominosNumber == 0) // pour garder le suspens, si le joueur a terminé on affiche 1 au lieu de 0 
                        chrominosNumber = 1;
                    pseudos_chrominos.Add(player.UserName, chrominosNumber);
                }
                string pictureName = Path.Combine(@"image\game", $"{GameDal.Details(game.Id).Guid}.png");
                listPictureGameVM.Add(new PictureGameVM(game.Id, pictureName, pseudos_chrominos, playerPseudoTurn, game.PlayedDate));
            }
            return listPictureGameVM;
        }

    }
}
