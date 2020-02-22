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
        protected Context Ctx { get; }
        protected GameDal GameDal { get; }
        protected GameChrominoDal GameChrominoDal { get; }
        protected ChrominoDal ChrominoDal { get; }
        protected PlayerDal PlayerDal { get; }
        protected GamePlayerDal GamePlayerDal { get; }
        protected SquareDal SquareDal { get; }
        protected int PlayerId { get; private set; }
        protected string PlayerPseudo { get; private set; }

        public CommonController(Context context)
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