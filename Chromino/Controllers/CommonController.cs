using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    public class CommonController : Controller
    {
        protected readonly IWebHostEnvironment Env;
        private Context context;

        protected Context Ctx { get; }
        protected GameDal GameDal { get; }
        protected ChrominoInGameDal ChrominoInGameDal { get; }
        protected ChrominoInHandDal ChrominoInHandDal { get; }
        protected ChrominoDal ChrominoDal { get; }
        protected PlayerDal PlayerDal { get; }
        protected GamePlayerDal GamePlayerDal { get; }
        protected SquareDal SquareDal { get; }
        protected int PlayerId { get; private set; }
        protected string PlayerPseudo { get; private set; }
        private UserManager<Player> UserManager {get; set; }

        public CommonController(Context context, UserManager<Player> userManager, IWebHostEnvironment env)
        {
            Env = env;
            Ctx = context;
            GameDal = new GameDal(Ctx);
            ChrominoInGameDal = new ChrominoInGameDal(Ctx);
            ChrominoInHandDal = new ChrominoInHandDal(Ctx);
            ChrominoDal = new ChrominoDal(Ctx);
            PlayerDal = new PlayerDal(Ctx);
            GamePlayerDal = new GamePlayerDal(Ctx);
            SquareDal = new SquareDal(Ctx);
            UserManager = userManager;
        }

        protected void GetPlayerInfos()
        {
            ViewBag.PlayerId = PlayerId = int.Parse(UserManager.GetUserId(User) ?? "0");
            ViewBag.PlayerPseudo = PlayerPseudo = UserManager.GetUserName(User) ?? "";
        }
    }
}