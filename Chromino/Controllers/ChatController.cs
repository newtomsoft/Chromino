using Controllers;
using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class ChatController : CommonController
    {
        protected ChatDal ChatDal { get; }

        public ChatController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
            ChatDal = new ChatDal(Ctx);
        }

        /// <summary>
        /// Envoie un message et le sauvegarde
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="message"></param>
        /// <returns>message formatté pour affichage</returns>
        [HttpPost]
        public IActionResult PostMessage(int gameId, string message)
        {
            if (PlayerIsInGame(gameId))
            {
                GamePlayer gamePlayer = GamePlayerDal.Details(gameId, PlayerId);
                ChatDal.Add(gamePlayer.Id, message);
                string newMessage = $"Vous ({DateTime.Now.ToString("dd/MM HH:mm").Replace(':', 'h')}) : {message}\n";
                GamePlayerDal.SetLatestReadMessage(gameId, PlayerId, DateTime.Now);
                return new JsonResult(new { newMessage });
            }
            else
                return null;
        }

        /// <summary>
        /// récupère dans le json de sortie les messages de la partie avec formattage
        /// </summary>
        /// <param name="gameId">id de la partie</param>
        /// <param name="onlyNewMessages">true pour obtenir uniquement les messages non lus</param>
        /// <param name="show">true pour remettre à 0 le compteur de nouveaux messages</param>
        /// <returns>chat: messages formattés, newMessagesNumber: nombre de messages non lus</returns>
        [HttpPost]
        public IActionResult GetMessages(int gameId, bool onlyNewMessages, bool show)
        {
            DateTime now = DateTime.Now;
            DateTime dateLatestRead = GamePlayerDal.GetLatestReadMessage(gameId, PlayerId);
            DateTime dateMin = onlyNewMessages ? dateLatestRead : DateTime.MinValue;
            DateTime dateMax = onlyNewMessages ? DateTime.MaxValue : dateLatestRead;
            List<Chat> chats = ChatDal.GetMessages(gameId, dateMin, dateMax);
            int newMessagesNumber = onlyNewMessages ? chats.Count() : ChatDal.NewMessagesNumber(gameId, dateLatestRead);
            Dictionary<int, string> gamePlayerId_PlayerName = GamePlayerDal.GamePlayersIdPlayerName(gameId);
            gamePlayerId_PlayerName[GamePlayerDal.Details(gameId, PlayerId).Id] = "Vous";
            string chat = "";
            foreach (Chat c in chats)
                chat += $"{gamePlayerId_PlayerName[c.GamePlayerId]} ({c.Date.ToString("dd/MM HH:mm").Replace(':', 'h')}) : {c.Message}\n";
            if (show)
                GamePlayerDal.SetLatestReadMessage(gameId, PlayerId, now);
            return new JsonResult(new { chat, newMessagesNumber });
        }
    }
}