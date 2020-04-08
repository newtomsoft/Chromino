using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChrominoApp.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public IActionResult OnGet()
        {
            TempData["errorMessage"] = "Vous devez créer un compte pour accéder à cette page";
            return LocalRedirect("/Identity/Account/Register");
        }
    }
}

