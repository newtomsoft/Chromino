using Data;
using Data.DAL;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using Tool;

namespace Controllers
{
    public class CommonController : Controller
    {
        protected IWebHostEnvironment Env { get; }

        protected Context Ctx { get; }
        protected GameDal GameDal { get; }
        protected ChrominoInGameDal ChrominoInGameDal { get; }
        protected ChrominoInHandDal ChrominoInHandDal { get; }
        protected ChrominoDal ChrominoDal { get; }
        protected PlayerDal PlayerDal { get; }
        protected GamePlayerDal GamePlayerDal { get; }
        protected SquareDal SquareDal { get; }
        protected int PlayerId { get => int.Parse(UserManager.GetUserId(User) ?? "0"); }
        protected string PlayerPseudo { get => UserManager.GetUserName(User) ?? ""; }
        protected UserManager<Player> UserManager { get; set; }

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

        /// <summary>
        /// fabrique la liste de PictureGameVM pour la vue
        /// </summary>
        /// <param name="games">liste des jeux</param>
        /// <returns></returns>
        protected List<PictureGameVM> MakePicturesGameVM(List<Game> games, bool keepSuspens = false)
        {
            List<PictureGameVM> listPictureGameVM = new List<PictureGameVM>();
            string picturePath = Path.Combine(Env.WebRootPath, "image/game");
            foreach (Game game in games)
            {
                PictureFactoryTool pictureFactoryTool = new PictureFactoryTool(game.Id, picturePath, Ctx);
                List<Player> players = GamePlayerDal.Players(game.Id);
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    int chrominosNumber = ChrominoInHandDal.ChrominosNumber(game.Id, player.Id);
                    if (keepSuspens && chrominosNumber == 0)
                        chrominosNumber = 1;
                    pseudos_chrominos.Add(player.UserName, chrominosNumber);
                }
                string pictureName = $"{GameDal.Details(game.Id).Guid}.png";
                if (!System.IO.File.Exists(Path.Combine(Env.WebRootPath, "image/game", pictureName)))
                    pictureFactoryTool.MakeThumbnail();

                listPictureGameVM.Add(new PictureGameVM(game.Id, pictureName, pseudos_chrominos, PlayerPseudo, game.PlayedDate));
            }
            return listPictureGameVM;
        }
    }
}