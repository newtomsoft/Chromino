using Controllers;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class ChatController : CommonController
    {
        public ChatController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        [HttpPost]
        public JsonResult Add(int gameId, string chat)
        {
            string newChat = $"{PlayerPseudo} ({DateTime.Now.ToString("dd/MM HH:mm").Replace(':', 'h')}) : {chat}\n";
            string resultChat = GameDal.UpdateChat(gameId, newChat, PlayerId);
            GameDal.SetChatRead(gameId, PlayerId);
            return new JsonResult(new { chat = resultChat });
        }

        [HttpPost]
        public void Read(int gameId)
        {
            GameDal.SetChatRead(gameId, PlayerId);
        }
    }
}