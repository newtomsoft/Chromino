using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChrominoApp.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public IActionResult OnGet()
        {
            TempData["errorMessage"] = "Veuillez créer un compte pour accéder à cette fonctionnalité";
            return LocalRedirect("/Identity/Account/Register");
        }
    }
}

