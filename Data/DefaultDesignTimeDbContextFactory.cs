using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Mooove.Data
{
    public class DefaultDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DefaultContext>
    {
        public DefaultContext CreateDbContext(string[] args)
        {
            string path = Directory.GetCurrentDirectory();

            IConfigurationBuilder builder = new ConfigurationBuilder()
                               .SetBasePath(path)
                               .AddJsonFile("appsettings.json");

            IConfigurationRoot config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultContext");
            DbContextOptionsBuilder<DefaultContext> optionBuilder = new DbContextOptionsBuilder<DefaultContext>();
            optionBuilder.UseSqlServer(connectionString);

            return new DefaultContext(optionBuilder.Options);
        }
    }
}