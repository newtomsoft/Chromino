using Controllers;
using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class SocialController : CommonController
    {
        protected PrivateMessageDal PrivateMessageDal { get; }

        public SocialController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
            PrivateMessageDal = new PrivateMessageDal(Ctx);
        }

        public IActionResult GetSendersIdUnreadMessagesNumber(int gameId)
        {
            var sendersId_UnreadMessagesNumber = PrivateMessageDal.GetSendersIdUnreadMessagesNumber(PlayerId);
            var unreadMessageNumber = new List<object>();
            foreach (var senderId_UnreadMessagesNumber in sendersId_UnreadMessagesNumber)
                unreadMessageNumber.Add(new { senderId_UnreadMessagesNumber.Key, senderId_UnreadMessagesNumber.Value });
            return new JsonResult(new { unreadMessageNumber });
        }

        /// <summary>
        /// Page des parties avec messages non lus
        /// </summary>
        /// <returns></returns>
        public IActionResult GamesWithUnreadMessages()
        {
            return View(MakePicturesGameVM(GamePlayerDal.GamesWithNotReadMessages(PlayerId)));
        }
    }
}