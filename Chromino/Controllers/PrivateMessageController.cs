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
    public class PrivateMessageController : CommonController
    {
        protected PrivateMessageDal PrivateMessageDal { get; }

        public PrivateMessageController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
            PrivateMessageDal = new PrivateMessageDal(Ctx);
        }

        /// <summary>
        /// Envoie un message et le sauvegarde
        /// </summary>
        /// <param name="recipientId">id du joueur destinataire</param>
        /// <param name="message">message brut</param>
        /// <returns>message formatté pour affichage</returns>
        [HttpPost]
        public JsonResult PostMessage(int recipientId, string message)
        {
            PrivateMessageDal.Add(PlayerId, recipientId, message);
            string newMessage = $"Vous ({DateTime.Now.ToString("dd/MM HH:mm").Replace(':', 'h')}) : {message}\n";
            PrivateMessageDal.SetDateLatestReadMessage(PlayerId, recipientId, DateTime.Now);
            return new JsonResult(new { newMessage });
        }

        /// <summary>
        /// récupère dans le json de sortie les messages privés avec formattage
        /// </summary>
        /// <param name="opponentId">id de l'adversaire avec qui s'échange les messages privés</param>
        /// <param name="onlyNewMessages">true pour obtenir uniquement les messages non lus</param>
        /// <param name="show">true pour remettre à 0 le compteur de nouveaux messages</param>
        /// <returns>message: messages formattés, newMessagesNumber: nombre de messages non lus</returns>
        [HttpPost]
        public JsonResult GetMessages(int opponentId, bool onlyNewMessages, bool show)
        {
            DateTime now = DateTime.Now;
            DateTime dateLatestRead = PrivateMessageDal.GetDateLatestReadMessage(PlayerId, opponentId);
            DateTime dateMin = onlyNewMessages ? dateLatestRead : DateTime.MinValue;
            List<PrivateMessage> privatesMessages = PrivateMessageDal.GetMessages(PlayerId, opponentId, dateMin, DateTime.MaxValue);
            int newMessagesNumber = onlyNewMessages ? privatesMessages.Count() : PrivateMessageDal.NewMessagesNumber(PlayerId, opponentId, dateLatestRead);
            string playerName = "Vous";
            string opponentName = PlayerDal.Name(opponentId);
            Dictionary<int, string> playerId_PlayerName = new Dictionary<int, string>();
            playerId_PlayerName.Add(PlayerId, playerName);
            playerId_PlayerName.Add(opponentId, opponentName);
            string message = "";
            foreach (PrivateMessage pm in privatesMessages)
                message += $"{playerId_PlayerName[pm.SenderId]} ({pm.Date.ToString("dd/MM HH:mm").Replace(':', 'h')}) : {pm.Message}\n";
            if (show)
                PrivateMessageDal.SetDateLatestReadMessage(PlayerId, opponentId, now);
            return new JsonResult(new { message, newMessagesNumber });
        }
    }
}