using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using Tool;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.Contracts;

namespace Controllers
{
    public class PlayerController : CommonController
    {
        public PlayerController(Context context) : base(context)
        {
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
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.error = "Votre pseudo ou mot de passe est incorrect. Merci de réessayer";
                    return View();
                }
                
            }
            else
            {
                ViewBag.error = "Votre pseudo ou mot de passe est incorrect. Merci de réessayer";
                return View();
            }
        }

        [HttpPost]
        public IActionResult LoginGuest()
        {
                Player found = PlayerDal.Details("Invité");
                if (found != null)
                {
                    HttpContext.Session.SetInt32(SessionKeyPlayerId, found.Id);
                    HttpContext.Session.SetString(SessionKeyPlayerPseudo, found.Pseudo);
                }
                return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await Ctx.Players
                .FirstOrDefaultAsync(m => m.Id == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Pseudo,Password,PlayedGames,WonGames")] Player player)
        {
            if (ModelState.IsValid)
            {
                Ctx.Add(player);
                await Ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(player);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await Ctx.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Pseudo,Password,PlayedGames,WonGames")] Player player)
        {
            if (id != player.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Ctx.Update(player);
                    await Ctx.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.Id))
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
            return View(player);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await Ctx.Players
                .FirstOrDefaultAsync(m => m.Id == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await Ctx.Players.FindAsync(id);
            Ctx.Players.Remove(player);
            await Ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlayerExists(int id)
        {
            return Ctx.Players.Any(e => e.Id == id);
        }
    }
}
