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
using Data.ViewModel;

namespace ChrominoGame.Controllers
{
    public class GameController : Controller
    {
        private readonly DefaultContext Ctx;
        private readonly GameDal GameDal;
        private readonly GameChrominoDal GameChrominoDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly SquareDal SquareDal;



        public GameController(DefaultContext context)
        {
            Ctx = context;
            GameDal = new GameDal(Ctx);
            GameChrominoDal = new GameChrominoDal(Ctx);
            ChrominoDal = new ChrominoDal(Ctx);
            PlayerDal = new PlayerDal(Ctx);
            GamePlayerDal = new GamePlayerDal(Ctx);
            SquareDal = new SquareDal(Ctx);
        }

        [HttpGet]
        public IActionResult StartNew()
        {
            return View(null);
        }


        [HttpPost]
        public IActionResult StartNew(string[] pseudos)
        {
            string error = null;
            Player playerBot = PlayerDal.Bot();

            List<Player> players = new List<Player>(8);
            if (pseudos[0] != null && pseudos[0].ToLower() != "bot")
            {
                Player player = PlayerDal.Detail(pseudos[0]);
                if (player != null)
                    players.Add(player);
                else
                    error = "pseudo 1 doesn't exist";
            }
            else
            {
                players.Add(playerBot);
            }
            for (int i = 1; i < pseudos.Length; i++)
            {
                if (pseudos[i] != null)
                {
                    Player player = PlayerDal.Detail(pseudos[i]);
                    if (player != null)
                        players.Add(player);
                    else
                        error = $"pseudo {i} doesn't exist";
                }
            }

            if (error != null)
            {
                ViewBag.error = error;
                return View(pseudos);
            }

            ChrominoDal.CreateChrominos();
            int gameId = GameDal.AddGame().Id;
            GamePlayerDal.Add(gameId, players);
            GameChrominoDal.Add(gameId);
            GameCore gamecore = new GameCore(Ctx, gameId, players);
            gamecore.BeginGame();
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        public IActionResult ContinueGame(int id)
        {
            List<Player> players = GamePlayerDal.Players(id);
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
            Player bot = PlayerDal.Bot();
            List<Player> players = new List<Player>(1) { bot };
            GameCore gamecore = new GameCore(Ctx, id, players);
            gamecore.ContinueRandomGame();
            return RedirectToAction("Show", "Game", new { id });
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
            GameDal.AddGame();
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

        public IActionResult Show(int id)
        {
            int chrominosInGame = GameChrominoDal.StatusNumber(id, ChrominoStatus.InGame);
            int chrominosInStack = GameChrominoDal.StatusNumber(id, ChrominoStatus.InStack);

            List<Player> players = GamePlayerDal.Players(id);
            List<int> numberChrominosInHand = new List<int>(players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                numberChrominosInHand.Add(GameChrominoDal.PlayerNumberChrominos(id, players[i].Id));
            }

            List<Chromino> identifiedPlayerChrominos = new List<Chromino>();
            if (players.Count == 1 && players[0].Pseudo == "Bot")
            {
                identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, players[0].Id);
            }

            Game game = GameDal.Details(id);
            GameStatus gameStatus = game.Status;
            bool autoPlay = game.AutoPlay;

            List<Square> squares = SquareDal.List(id);
            GameViewModel gameViewModel = new GameViewModel(id, squares, autoPlay, gameStatus, chrominosInGame, chrominosInStack, numberChrominosInHand, identifiedPlayerChrominos);

            return View(gameViewModel);
        }

        [HttpPost]
        public IActionResult AutoPlay(int gameId, bool autoPlay)
        {
            GameDal.SetAutoPlay(gameId, autoPlay);
            return RedirectToAction("ContinueRandomGame", "Game", new { id = gameId });
        }






        private bool GameExists(int id)
        {
            return Ctx.Games.Any(e => e.Id == id);
        }
    }
}
