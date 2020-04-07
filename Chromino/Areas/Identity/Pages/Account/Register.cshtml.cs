using Data.DAL;
using Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ChrominoApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Player> _signInManager;
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<Player> userManager,
            SignInManager<Player> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel //: IValidatableObject
        {
            [Required(ErrorMessage = "Le nom de joueur est obligatoire")]
            [StringLength(50, ErrorMessage = "Le {0} doit avoir au moins {2} et au maximum {1} caractères.", MinimumLength = 2)]
            [Display(Name = "Nom de joueur")]
            public string PlayerName { get; set; }

            [Required(ErrorMessage = "Le mot de passe est obligatoire")]
            [StringLength(20, ErrorMessage = "Le {0} doit avoir au moins {2} et au maximum {1} caractères.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mot de passe")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmez le mot de passe")]
            [Compare("Password", ErrorMessage = "Les 2 mots de passe ne sont pas identiques.")]
            public string ConfirmPassword { get; set; }

            [EmailAddress(ErrorMessage = "Merci de rentrer une adresse email valide ou laisser le champ vide")]
            [StringLength(50, ErrorMessage = "Le {0} doit avoir au moins {2} et au maximum {1} caractères.", MinimumLength = 2)]
            [Display(Name = "Email (optionnel)")]
            public string Email { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                if (Input.Email != null)
                {
                    Player player = _userManager.FindByEmailAsync(Input.Email).Result;
                    if (player != null)
                    {
                        ModelState.AddModelError("Email", "Un joueur avec cette adresse email est déjà inscrit");
                        return Page();
                    }
                }

                var user = new Player { UserName = Input.PlayerName, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    int i = 1;
                    string addDescription = "";
                    if (error.Code == "DuplicateUserName")
                    {
                        while (_userManager.FindByNameAsync(user.UserName + i).Result != null)
                            i++;
                        addDescription = $"\nSuggestion : {user.UserName + i}";
                    }
                    ModelState.AddModelError(string.Empty, error.Description + addDescription);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
