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
using Microsoft.AspNetCore.Mvc.Rendering;
using Data.ViewModel;

namespace Controllers
{
    public class HomeController : CommonController
    {
        public HomeController(Context ctx) : base(ctx)
        {
        }

        public IActionResult Index()
        {
            GetPlayerInfosFromSession();
            if (PlayerId == 0)
            {
                return RedirectToAction("Login", "Player");
            }
            else
            {
                List<SelectListItem> listSelectListItem = new List<SelectListItem>();
                SelectListItem intro = new SelectListItem() { Value = "selected", Text = "Play multi game", Disabled = true };
                listSelectListItem.Add(intro);
                foreach (Game game in GamePlayerDal.MultiGamesToPlay(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                    SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                    listSelectListItem.Add(selectListItem);
                }
                ViewData["GamesToPlay"] = new SelectList(listSelectListItem, "Value", "Text");

                listSelectListItem = new List<SelectListItem>();
                intro = new SelectListItem() { Value = "selected", Text = "Play alone game", Disabled = true };
                listSelectListItem.Add(intro);
                foreach (Game game in GamePlayerDal.SingleGamesInProgress(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                    SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                    listSelectListItem.Add(selectListItem);
                }
                ViewData["SingleGames"] = new SelectList(listSelectListItem, "Value", "Text");

                listSelectListItem = new List<SelectListItem>();
                intro = new SelectListItem() { Value = "selected", Text = "View won game", Disabled = true };
                listSelectListItem.Add(intro);
                foreach (Game game in GamePlayerDal.GamesWon(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                    SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                    listSelectListItem.Add(selectListItem);
                }
                ViewData["GamesWon"] = new SelectList(listSelectListItem, "Value", "Text");

                listSelectListItem = new List<SelectListItem>();
                intro = new SelectListItem() { Value = "selected", Text = "View lost game", Disabled = true };
                listSelectListItem.Add(intro);
                foreach (Game game in GamePlayerDal.GamesLost(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                    SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                    listSelectListItem.Add(selectListItem);
                }
                ViewData["GamesLost"] = new SelectList(listSelectListItem, "Value", "Text");

                listSelectListItem = new List<SelectListItem>();
                intro = new SelectListItem() { Value = "selected", Text = "View finished alone game", Disabled = true };
                listSelectListItem.Add(intro);
                foreach (Game game in GamePlayerDal.SingleGamesFinished(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                    SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                    listSelectListItem.Add(selectListItem);
                }
                ViewData["SingleGamesFinished"] = new SelectList(listSelectListItem, "Value", "Text");

                listSelectListItem = new List<SelectListItem>();
                intro = new SelectListItem() { Value = "selected", Text = "View opponent turn game", Disabled = true };
                listSelectListItem.Add(intro);
                foreach (Game game in GamePlayerDal.GamesWaitTurn(PlayerId))
                {
                    List<Player> players = GamePlayerDal.Players(game.Id);
                    string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).Pseudo;
                    Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                    foreach (Player player in players)
                    {
                        pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id));
                    }
                    GameForListVM gameForListVM = new GameForListVM(game, pseudos_chrominos, playerPseudoTurn);
                    SelectListItem selectListItem = new SelectListItem() { Value = gameForListVM.GameId.ToString(), Text = gameForListVM.Infos };
                    listSelectListItem.Add(selectListItem);
                }
                ViewData["GamesWaitTurn"] = new SelectList(listSelectListItem, "Value", "Text");

                


                return View();
            }
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
