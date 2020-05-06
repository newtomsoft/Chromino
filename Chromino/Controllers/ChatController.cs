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
        public JsonResult PostMessage(int gameId, string message)
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
        /// <param name="newMessages">true pour obtenir uniquement les messages non lus</param>
        /// <param name="resetNotification">true pour remettre à 0 le compteur de nouveaux messages</param>
        /// <returns>Json avec chat: messages formattés, newMessagesNumber: nombre de messages non lus</returns>
        [HttpPost]
        public JsonResult GetMessages(int gameId, bool newMessages = false, bool resetNotification = false)
        {
            DateTime now = DateTime.Now;
            DateTime dateLatestRead = GamePlayerDal.GetLatestReadMessage(gameId, PlayerId);
            DateTime dateMin, dateMax;
            if (newMessages)
            {
                dateMin = dateLatestRead;
                dateMax = DateTime.MaxValue;
            }
            else
            {
                dateMin = DateTime.MinValue;
                dateMax = dateLatestRead;
            }
            Dictionary<int, string> gamePlayerId_PlayerName = GamePlayerDal.GamePlayersIdPlayerName(gameId);
            gamePlayerId_PlayerName[GamePlayerDal.Details(gameId, PlayerId).Id] = "Vous";
            List<string> playersName = new List<string>();
            var chats = ChatDal.GetMessages(gameId, dateMin, dateMax);
            string chat = "";
            foreach (Chat c in chats)
                chat += $"{gamePlayerId_PlayerName[c.GamePlayerId]} ({c.Date.ToString("dd/MM HH:mm").Replace(':', 'h')}) : {c.Message}\n";
            int newMessagesNumber;
            if (resetNotification)
                GamePlayerDal.SetLatestReadMessage(gameId, PlayerId, now);
            if (newMessages)
                newMessagesNumber = chats.Count();
            else
                newMessagesNumber = ChatDal.NewMessagesNumber(gameId, dateLatestRead);


            return new JsonResult(new { chat, newMessagesNumber });
        }

        /// <summary>
        /// indique que les message à la date actuelle ont été lus
        /// </summary>
        /// <param name="gameId"></param>
        [HttpPost]
        public void SetLatestReadMessage(int gameId)
        {
            if (PlayerIsInGame(gameId))
            {
                GamePlayerDal.SetLatestReadMessage(gameId, PlayerId, DateTime.Now);
            }
        }
    }
}