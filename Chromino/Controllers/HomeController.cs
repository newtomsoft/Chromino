using ChrominoGame.Models;
using Data;
using Data.DAL;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Diagnostics;

namespace Controllers
{
    [Authorize]
    public class HomeController : CommonController
    {
        public HomeController(Context context, UserManager<Player> userManager) : base(context, userManager)
        {
        }

        /// <summary>
        /// Pages des parties à jouer
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            List<SelectListItem> listSelectListItem = new List<SelectListItem>();
            SelectListItem intro = new SelectListItem() { Value = "selected", Text = "Parties à plusieurs (votre tour)", Disabled = true };
            listSelectListItem.Add(intro);
            foreach (Game game in GamePlayerDal.MultiGamesToPlay(PlayerId))
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    int chrominosNumber = GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id);
                    if (chrominosNumber == 0) // pour garder le suspens, on affiche 1 au lieu de 0 si le joueur a terminé
                        chrominosNumber = 1;
                    pseudos_chrominos.Add(player.UserName, chrominosNumber);
                }
                GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                listSelectListItem.Add(selectListItem);
            }
            ViewData["GamesToPlay"] = new SelectList(listSelectListItem, "Value", "Text");

            listSelectListItem = new List<SelectListItem>();
            intro = new SelectListItem() { Value = "selected", Text = "Parties seul", Disabled = true };
            listSelectListItem.Add(intro);
            foreach (Game game in GamePlayerDal.SingleGamesInProgress(PlayerId))
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    pseudos_chrominos.Add(player.UserName, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                }
                GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                listSelectListItem.Add(selectListItem);
            }
            ViewData["SingleGames"] = new SelectList(listSelectListItem, "Value", "Text");

            return View();
        }

        /// <summary>
        /// Page des parties en cours (tour d'un adversaire)
        /// </summary>
        /// <returns></returns>
        public IActionResult GamesInProgress()
        {
            List<SelectListItem> listSelectListItem = new List<SelectListItem>();
            SelectListItem intro = new SelectListItem() { Value = "selected", Text = "Parties en attente (tour d'un adversaire)", Disabled = true };
            listSelectListItem.Add(intro);
            foreach (Game game in GamePlayerDal.GamesWaitTurn(PlayerId))
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    pseudos_chrominos.Add(player.UserName, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                }
                GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                listSelectListItem.Add(selectListItem);
            }
            ViewData["GamesWaitTurn"] = new SelectList(listSelectListItem, "Value", "Text");
            return View();
        }

        /// <summary>
        /// Page des parties terminées
        /// </summary>
        /// <returns></returns>
        public IActionResult GamesFinished()
        {
            List<SelectListItem> listSelectListItem = new List<SelectListItem>();
            SelectListItem intro = new SelectListItem() { Value = "selected", Text = "Parties gagnées", Disabled = true };
            listSelectListItem.Add(intro);
            foreach (Game game in GamePlayerDal.GamesWon(PlayerId))
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    pseudos_chrominos.Add(player.UserName, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                }
                GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                listSelectListItem.Add(selectListItem);
            }
            ViewData["GamesWon"] = new SelectList(listSelectListItem, "Value", "Text");

            listSelectListItem = new List<SelectListItem>();
            intro = new SelectListItem() { Value = "selected", Text = "Parties perdues", Disabled = true };
            listSelectListItem.Add(intro);
            foreach (Game game in GamePlayerDal.GamesLost(PlayerId))
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    pseudos_chrominos.Add(player.UserName, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                }
                GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                listSelectListItem.Add(selectListItem);
            }
            ViewData["GamesLost"] = new SelectList(listSelectListItem, "Value", "Text");

            listSelectListItem = new List<SelectListItem>();
            intro = new SelectListItem() { Value = "selected", Text = "Parties seul terminées", Disabled = true };
            listSelectListItem.Add(intro);
            foreach (Game game in GamePlayerDal.SingleGamesFinished(PlayerId))
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    pseudos_chrominos.Add(player.UserName, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                }
                GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                listSelectListItem.Add(selectListItem);
            }
            ViewData["SingleGamesFinished"] = new SelectList(listSelectListItem, "Value", "Text");
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Rgpd()
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
