using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using Data.Core;
using Data.DAL;
using Data.Enumeration;

namespace Chromino.Controllers
{
    public class GameController : Controller
    {
        private readonly DefaultContext Ctx;
        private readonly GameDal GameDAL;
        private readonly GameChrominoDal ChrominoGameDAL;
        private readonly ChrominoDal ChrominoDAL;
        private readonly PlayerDal PlayerDAL;
        private readonly GamePlayerDal GamePlayerDAL;
        public GameController(DefaultContext context)
        {
            Ctx = context;
            GameDAL = new GameDal(Ctx);
            ChrominoGameDAL = new GameChrominoDal(Ctx);
            ChrominoDAL = new ChrominoDal(Ctx);
            PlayerDAL = new PlayerDal(Ctx);
            GamePlayerDAL = new GamePlayerDal(Ctx);
        }

        [HttpGet]
        public IActionResult StartNew()
        {
            return View();
        }


        [HttpPost]
        public IActionResult StartNew(int player1Id, int? player2Id, int? player3Id, int? player4Id, int? player5Id, int? player6Id, int? player7Id, int? player8Id)
        {
            //TODO test player Bot
            Player bot = PlayerDAL.Bot();

            List<Player> players = new List<Player>(8);
            players.Add(PlayerDAL.Detail(player1Id));
            if (player2Id != null)
                players.Add(PlayerDAL.Detail((int)player2Id));
            if (player3Id != null)
                players.Add(PlayerDAL.Detail((int)player3Id));
            if (player4Id != null)
                players.Add(PlayerDAL.Detail((int)player4Id));
            if (player5Id != null)
                players.Add(PlayerDAL.Detail((int)player5Id));
            if (player6Id != null)
                players.Add(PlayerDAL.Detail((int)player6Id));
            if (player7Id != null)
                players.Add(PlayerDAL.Detail((int)player7Id));
            if (player8Id != null)
                players.Add(PlayerDAL.Detail((int)player8Id));

            List<int> playersId = players.Select(x => x.Id).ToList();
            ChrominoDAL.CreateChrominos();
            int gameId = GameDAL.AddGame().Id;
            GamePlayerDAL.Add(gameId, playersId);
            ChrominoGameDAL.Add(gameId);
            GameCore gamecore = new GameCore(Ctx, gameId, players);
            gamecore.BeginGame();
            return RedirectToAction("Show", "Grid", new { id = gameId });
        }

        public IActionResult ContinueGame(int id)
        {
            List<Player> players = GamePlayerDAL.Players(id);
            if (players.Count == 1 && players[0].Pseudo == "bot")
            {
                return RedirectToAction("ContinueRandomGame", "Game", new { id });
            }
            else
            {
                // TODO jeux "normal"
                return View();
            }
        }

        public IActionResult ContinueRandomGame(int id)
        {
            Player bot = PlayerDAL.Bot();
            List<Player> players = new List<Player>(1) { bot };
            GameCore gamecore = new GameCore(Ctx, id, players);
            gamecore.ContinueRandomGame();
            return RedirectToAction("Show", "Grid", new { id });
        }


        // GET: Game
        public async Task<IActionResult> Index()
        {
            return View(await Ctx.Games.ToListAsync());
        }

        // GET: Game/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await Ctx.Games
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Game/Create
        public IActionResult Create()
        {
            GameDAL.AddGame();
            return View();
        }


        // GET: Game/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await Ctx.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // POST: Game/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Guid,CreateDate")] Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Ctx.Update(game);
                    await Ctx.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Game/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await Ctx.Games
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Game/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await Ctx.Games.FindAsync(id);
            Ctx.Games.Remove(game);
            await Ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return Ctx.Games.Any(e => e.Id == id);
        }
    }
}
