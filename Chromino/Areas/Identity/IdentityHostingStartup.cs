using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ChrominoApp.Areas.Identity.IdentityHostingStartup))]
namespace ChrominoApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}