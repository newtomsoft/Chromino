using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;

namespace ChrominoGame.Controllers
{
    public class Game_PlayerController : Controller
    {
        private readonly DefaultContext _context;

        public Game_PlayerController(DefaultContext context)
        {
            _context = context;
        }

        // GET: GamePlayer
        public async Task<IActionResult> Index()
        {
            var defaultContext = _context.Games_Players.Include(g => g.Game).Include(g => g.Player);
            return View(await defaultContext.ToListAsync());
        }

        // GET: GamePlayer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game_Player = await _context.Games_Players
                .Include(g => g.Game)
                .Include(g => g.Player)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game_Player == null)
            {
                return NotFound();
            }

            return View(game_Player);
        }

        // GET: GamePlayer/Create
        public IActionResult Create()
        {
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id");
            ViewData["PlayerId"] = new SelectList(_context.Players, "Id", "Pseudo");
            return View();
        }

        // POST: GamePlayer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GameId,PlayerId,PlayerTurn,PlayerPoints")] GamePlayer game_Player)
        {
            if (ModelState.IsValid)
            {
                _context.Add(game_Player);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id", game_Player.GameId);
            ViewData["PlayerId"] = new SelectList(_context.Players, "Id", "Pseudo", game_Player.PlayerId);
            return View(game_Player);
        }

        // GET: GamePlayer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game_Player = await _context.Games_Players.FindAsync(id);
            if (game_Player == null)
            {
                return NotFound();
            }
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id", game_Player.GameId);
            ViewData["PlayerId"] = new SelectList(_context.Players, "Id", "Pseudo", game_Player.PlayerId);
            return View(game_Player);
        }

        // POST: GamePlayer/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GameId,PlayerId,PlayerTurn,PlayerPoints")] GamePlayer game_Player)
        {
            if (id != game_Player.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game_Player);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Game_PlayerExists(game_Player.Id))
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
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id", game_Player.GameId);
            ViewData["PlayerId"] = new SelectList(_context.Players, "Id", "Pseudo", game_Player.PlayerId);
            return View(game_Player);
        }

        // GET: GamePlayer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game_Player = await _context.Games_Players
                .Include(g => g.Game)
                .Include(g => g.Player)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game_Player == null)
            {
                return NotFound();
            }

            return View(game_Player);
        }

        // POST: GamePlayer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game_Player = await _context.Games_Players.FindAsync(id);
            _context.Games_Players.Remove(game_Player);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Game_PlayerExists(int id)
        {
            return _context.Games_Players.Any(e => e.Id == id);
        }
    }
}
