using Controllers;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class MemoController : CommonController
    {
        public MemoController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        [HttpPost]
        public JsonResult Add(int gameId, string memo)
        {
            if (memo != null)
                memo = memo.Trim();
            GamePlayerDal.ChangeMemo(gameId, PlayerId, memo);
            return new JsonResult(new { memosNumber = memo?.Count(x => x == '\n') + 1 ?? 0 });
        }
    }
}