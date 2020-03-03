using Data;
using Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
            services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = this.Configuration["apis:facebook:id"];
                options.AppSecret = this.Configuration["apis:facebook:secret"];
            });

            IMvcBuilder builder = services.AddRazorPages();
#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif
            services.AddControllersWithViews();
            services.AddDbContext<Context>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultContext")), ServiceLifetime.Scoped);
            services.AddIdentity<Player, Role>()
                .AddEntityFrameworkStores<Context>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddOptions();
            //services.Configure<MyConfig>(Configuration.GetSection("MyConfig"));
            services.ConfigureApplicationCookie(options => options.LoginPath = "/Identity/Account/Login");
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
