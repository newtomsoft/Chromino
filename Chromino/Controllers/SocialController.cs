using Controllers;
using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public IActionResult Index(int gameId)
        {
            var unreadMessagesSendersTemp = PrivateMessageDal.GetUnreadMessagesSenders(PlayerId);
            string unreadMessagesSenders = JsonConvert.SerializeObject(unreadMessagesSendersTemp);
            return new JsonResult(new { unreadMessagesSenders });
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