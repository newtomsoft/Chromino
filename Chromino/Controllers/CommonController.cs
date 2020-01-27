using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    public class CommonController : Controller
    {
        protected const string SessionKeyPlayerId = "PlayerId";
        protected const string SessionKeyPlayerPseudo = "PlayerPseudo";
        protected readonly DefaultContext Ctx;
        protected readonly GameDal GameDal;
        protected readonly GameChrominoDal GameChrominoDal;
        protected readonly ChrominoDal ChrominoDal;
        protected readonly PlayerDal PlayerDal;
        protected readonly GamePlayerDal GamePlayerDal;
        protected readonly SquareDal SquareDal;
        protected int PlayerId { get; private set; }
        protected string PlayerPseudo { get; private set; }

        public CommonController(DefaultContext context)
        {
            Ctx = context;
            GameDal = new GameDal(Ctx);
            GameChrominoDal = new GameChrominoDal(Ctx);
            ChrominoDal = new ChrominoDal(Ctx);
            PlayerDal = new PlayerDal(Ctx);
            GamePlayerDal = new GamePlayerDal(Ctx);
            SquareDal = new SquareDal(Ctx);
        }

        protected void GetPlayerInfosFromSession()
        {
            GetPlayerIdFromSession();
            GetPlayerPseudoFromSession();
        }

        private void GetPlayerIdFromSession()
        {
            ViewBag.PlayerId = PlayerId = HttpContext.Session.GetInt32(SessionKeyPlayerId) ?? 0;
        }

        private void GetPlayerPseudoFromSession()
        {
            ViewBag.PlayerPseudo = PlayerPseudo = HttpContext.Session.GetString(SessionKeyPlayerPseudo) ?? "";
        }
    }
}