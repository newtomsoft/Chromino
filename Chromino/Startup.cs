using Data;
using Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Hubs;
using Tool;

namespace ChrominoGame
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            #region apiAddAuthentication
            string googleClientId = Configuration["apis:google:ClientId"];
            string googleClientSecret = Configuration["apis:google:ClientSecret"];
            if (googleClientId != null && googleClientSecret != null)
            {
                services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                });
            }
            string twitterConsumerKey = Configuration["apis:twitter:ConsumerKey"];
            string twitterConsumerSecret = Configuration["apis:twitter:ConsumerSecret"];
            if (twitterConsumerKey != null && twitterConsumerSecret != null)
            {
                services.AddAuthentication().AddTwitter(options =>
                {
                    options.ConsumerKey = twitterConsumerKey;
                    options.ConsumerSecret = twitterConsumerSecret;
                });
            }
            string facebookAppId = Configuration["apis:facebook:AppId"];
            string facebookAppSecret = Configuration["apis:facebook:AppSecret"];
            if (facebookAppId != null && facebookAppSecret != null)
            {
                services.AddAuthentication().AddFacebook(options =>
                {
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                    options.CallbackPath = "/signin-facebook";
                });
            }
            string linkedinAppId = Configuration["apis:linkedin:ClientId"];
            string linkedinAppSecret = Configuration["apis:linkedin:ClientSecret"];
            if (linkedinAppId != null && linkedinAppSecret != null)
            {
                services.AddAuthentication().AddLinkedIn(options =>
                {
                    options.ClientId = linkedinAppId;
                    options.ClientSecret = linkedinAppSecret;
                });
            }
            string githubAppId = Configuration["apis:github:ClientId"];
            string githubAppSecret = Configuration["apis:github:ClientSecret"];
            if (githubAppId != null && githubAppSecret != null)
            {
                services.AddAuthentication().AddGitHub(options =>
                {
                    options.ClientId = githubAppId;
                    options.ClientSecret = githubAppSecret;
                });
            }
            string discordAppId = Configuration["apis:discord:ClientId"];
            string discordAppSecret = Configuration["apis:discord:ClientSecret"];
            if (discordAppId != null && discordAppSecret != null)
            {
                services.AddAuthentication().AddDiscord(options =>
                {
                    options.ClientId = discordAppId;
                    options.ClientSecret = discordAppSecret;
                });
            }
            string slackAppId = Configuration["apis:slack:ClientId"];
            string slackAppSecret = Configuration["apis:slack:ClientSecret"];
            if (slackAppId != null && slackAppSecret != null)
            {
                services.AddAuthentication().AddSlack(options =>
                {
                    options.ClientId = slackAppId;
                    options.ClientSecret = slackAppSecret;
                });
            }
            string deezerAppId = Configuration["apis:deezer:ClientId"];
            string deezerAppSecret = Configuration["apis:deezer:ClientSecret"];
            if (deezerAppId != null && deezerAppSecret != null)
            {
                services.AddAuthentication().AddDeezer(options =>
                {
                    options.ClientId = deezerAppId;
                    options.ClientSecret = deezerAppSecret;
                });
            }
            string spotifyAppId = Configuration["apis:spotify:ClientId"];
            string spotifyAppSecret = Configuration["apis:spotify:ClientSecret"];
            if (spotifyAppId != null && spotifyAppSecret != null)
            {
                services.AddAuthentication().AddSpotify(options =>
                {
                    options.ClientId = spotifyAppId;
                    options.ClientSecret = spotifyAppSecret;
                });
            }
            #endregion
            IMvcBuilder builder = services.AddRazorPages();
#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif
            services.AddControllersWithViews();
            services.AddDbContext<Context>(options => options.UseSqlServer(Configuration.GetConnectionString("UserDbContext")), ServiceLifetime.Scoped);
            services.AddIdentity<Player, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 2;
            })
            .AddRoleManager<RoleManager<IdentityRole<int>>>()
            .AddEntityFrameworkStores<Context>()
            .AddDefaultTokenProviders()
            .AddDefaultUI()
            .AddErrorDescriber<ChrominoIdentityErrorDescriber>();
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddOptions();
            services.ConfigureApplicationCookie(options => options.LoginPath = "/Identity/Account/Login");
            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if DEBUG
            app.UseDeveloperExceptionPage();
#else
            app.UseExceptionHandler("/Home/Error");
#endif
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "Admin",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<HubSignal>("/hubGame");
            });
        }
    }
}
