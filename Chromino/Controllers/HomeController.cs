using ChrominoGame.Models;
using Data;
using Data.Core;
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
            ViewData["GamesToPlay"] = MakePicturesGameVM(GamePlayerDal.MultiGamesToPlay(PlayerId), true);
            ViewData["SingleGames"] = MakePicturesGameVM(GamePlayerDal.SingleGamesInProgress(PlayerId));
            return View();
        }

        /// <summary>
        /// Page des parties en cours (tour d'un adversaire)
        /// </summary>
        /// <returns></returns>
        public IActionResult GamesInProgress()
        {
            GetPlayerInfos();
            ViewData["GamesWaitTurn"] = MakePicturesGameVM(GamePlayerDal.GamesWaitTurn(PlayerId), true);
            return View();
        }

        /// <summary>
        /// Page des parties terminées
        /// </summary>
        /// <returns></returns>
        public IActionResult GamesFinished()
        {
            GetPlayerInfos();
            ViewData["GamesWon"] = MakePicturesGameVM(GamePlayerDal.GamesWon(PlayerId));
            ViewData["GamesLost"] = MakePicturesGameVM(GamePlayerDal.GamesLost(PlayerId));
            ViewData["SingleGamesFinished"] = MakePicturesGameVM(GamePlayerDal.SingleGamesFinished(PlayerId));
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
        private List<PictureGameVM> MakePicturesGameVM(List<Game> games, bool keepSuspens = false)
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
                    if (keepSuspens && chrominosNumber == 0)
                        chrominosNumber = 1;
                    pseudos_chrominos.Add(player.UserName, chrominosNumber);
                }
                string pictureName = $"{GameDal.Details(game.Id).Guid}.png";
                if (!System.IO.File.Exists(Path.Combine(Env.WebRootPath, @"image/game", pictureName)))
                    new GameCore(Ctx, Env, game.Id).MakePicture();
                listPictureGameVM.Add(new PictureGameVM(game.Id, pictureName, pseudos_chrominos, playerPseudoTurn, game.PlayedDate));
            }
            return listPictureGameVM;
        }

    }
}
