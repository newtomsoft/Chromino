using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Controllers;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class ChatController : CommonController
    {
        public ChatController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        [HttpPost]
        public IActionResult Add(int gameId, string playerPseudo, string chat)
        {
            if (playerPseudo == PlayerPseudo)
            {
                string newChat = $"{playerPseudo} ({DateTime.Now.ToString("dd/MM HH:mm").Replace(':', 'h')}) : {chat}\n";
                GameDal.UpdateChat(gameId, newChat, PlayerId);
                GameDal.SetChatRead(gameId, PlayerId);
                return RedirectToAction("Show", "Game", new { id = gameId });
            }
            else
            {
                return RedirectToAction("NotFound");
            }
        }

        [HttpPost]
        public IActionResult SetRead(int gameId, int playerId)
        {
            if (playerId == PlayerId)
            {
                GameDal.SetChatRead(gameId, playerId);
                return RedirectToAction("Show", "Game", new { id = gameId });
            }
            else
            {
                return RedirectToAction("NotFound");
            }

        }
    }
}