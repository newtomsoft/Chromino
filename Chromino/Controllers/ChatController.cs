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
        protected PrivateMessageDal PrivateMessageDal { get; }

        public ChatController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
            ChatDal = new ChatDal(Ctx);
            PrivateMessageDal = new PrivateMessageDal(Ctx);
        }

        /// <summary>
        /// Envoie un message et le sauvegarde
        /// </summary>
        /// <param name="type">type de message "chatGame" ou "private"</param>
        /// <param name="id">id de la partie ou du destinataire</param>
        /// <param name="message">message brut</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PostMessage(string type, int id, string message)
        {
            if (type == "chatGame")
            {
                if (!PlayerIsInGame(id))
                    return null;
                int gamePlayerId = GamePlayerDal.Details(id, PlayerId).Id;
                ChatDal.Add(gamePlayerId, message);
                GamePlayerDal.SetDateLatestReadMessage(PlayerId, id, DateTime.Now);
            }
            else if (type == "private")
            {
                PrivateMessageDal.Add(PlayerId, id, message);
                PrivateMessageDal.SetDateLatestReadMessage(id, PlayerId, DateTime.Now); // inversion sender, recipient normal car le recipient est ici le sender
            }
            var newMessage = new
            {
                playerName = "Vous",
                date = DateTime.Now.ToString("dd/MM HH:mm").Replace(':', 'h'),
                message = message,
            };
            return new JsonResult(new { message = newMessage });
        }

        /// <summary>
        /// récupère dans le json de sortie les messages de la partie avec formattage
        /// </summary>
        /// <param name="gameId">id de la partie</param>
        /// <param name="onlyNewMessages">true pour obtenir uniquement les messages non lus</param>
        /// <param name="show">true pour remettre à 0 le compteur de nouveaux messages</param>
        /// <returns></returns>
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
            var messages = (from c in chats
                            select new
                            {
                                playerName = gamePlayerId_PlayerName[c.GamePlayerId],
                                date = c.Date.ToString("dd/MM HH:mm").Replace(':', 'h'),
                                message = c.Message
                            }).ToList();
            if (show)
                GamePlayerDal.SetDateLatestReadMessage(PlayerId, gameId, now);
            return new JsonResult(new { messages, newMessagesNumber });
        }

        public JsonResult GetPrivatesMessages(int opponentId, bool onlyNewMessages, bool show)
        {
            DateTime now = DateTime.Now;
            DateTime dateLatestRead = PrivateMessageDal.GetDateLatestReadMessage(opponentId, PlayerId);
            DateTime dateMin = onlyNewMessages ? dateLatestRead : DateTime.MinValue;
            List<PrivateMessage> privatesMessages = PrivateMessageDal.GetMessages(PlayerId, opponentId, dateMin, DateTime.MaxValue);
            int newMessagesNumber = onlyNewMessages ? privatesMessages.Count() : PrivateMessageDal.NewMessagesNumber(PlayerId, opponentId, dateLatestRead);
            string playerName = "Vous";
            string opponentName = PlayerDal.Name(opponentId);
            Dictionary<int, string> playerId_PlayerName = new Dictionary<int, string>();
            playerId_PlayerName.Add(PlayerId, playerName);
            playerId_PlayerName.Add(opponentId, opponentName);
            var messages = (from pm in privatesMessages
                            select new
                            {
                                playerName = playerId_PlayerName[pm.SenderId],
                                date = pm.Date.ToString("dd/MM HH:mm").Replace(':', 'h'),
                                message = pm.Message
                            }).ToList();

            if (show)
                PrivateMessageDal.SetDateLatestReadMessage(opponentId, PlayerId, now);
            return new JsonResult(new { messages, newMessagesNumber });
        }

        public JsonResult GetNewPrivatesMessagesNumber()
        {
            Dictionary<int, DateTime> recipientsId_dates = PrivateMessageDal.GetLatestReadMessageRecipientsId_Dates(PlayerId);
            var sendersId_newMessagesNumber = new Dictionary<int, int>();
            foreach (var recipientId_date in recipientsId_dates)
            {
                int opponentId = recipientId_date.Key;
                DateTime dateLatestRead = recipientId_date.Value;
                sendersId_newMessagesNumber.Add(opponentId, PrivateMessageDal.NewMessagesNumber(PlayerId, opponentId, dateLatestRead));
            }
            var data = (from c in sendersId_newMessagesNumber
                        select new
                        {
                            sendersId = c.Key,
                            newMessagesNumber = c.Value,
                        }).ToList();

            return new JsonResult(data);
        }
    }
}