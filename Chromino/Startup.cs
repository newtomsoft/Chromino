using Data;
using Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalR.Hubs;
using System;
using System.Text;
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
