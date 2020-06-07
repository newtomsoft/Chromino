using Data;
using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace Controllers
{
    public class TipController : CommonController
    {
        protected TipDal TipDal { get; }
        public TipController(Context context, UserManager<Player> userManager, SignInManager<Player> signInManager) : base(context, userManager, null)
        {
            TipDal = new TipDal(Ctx);
        }

        [HttpPost]
        public IActionResult Off(int gameId, int tipId, bool dontShowAllTips)
        {
            if (dontShowAllTips)
                TipDal.SetAllOff(PlayerId);
            else
                TipDal.SetOff(PlayerId, tipId);
            return RedirectToAction("Show", "Game", new { id = gameId });
        }
    }
}
