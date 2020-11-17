using Microsoft.EntityFrameworkCore.Design;
using Newtomsoft.Tools;

namespace Data
{
    public class DefaultDesignTimeDbContextFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args)
        {
            return EntityFrameworkTools<Context>.CreateDbContext("Chromino");

            //string path = Path.Combine(Directory.GetCurrentDirectory(), "..", "Chromino");
            //string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //if (env == null)
            //    env = "Development";
            //Console.WriteLine("ASPNETCORE_ENVIRONMENT is : " + env);
            //IConfigurationBuilder builder = new ConfigurationBuilder()
            //                   .SetBasePath(path)
            //                   .AddJsonFile($"appsettings.{env}.json");
            //IConfigurationRoot config = builder.Build();
            //string connectionString = config.GetConnectionString("AdminDbContext");
            //DbContextOptionsBuilder<Context> optionBuilder = new DbContextOptionsBuilder<Context>();
            //optionBuilder.UseSqlServer(connectionString);
            //return new Context(optionBuilder.Options);
        }
    }
}