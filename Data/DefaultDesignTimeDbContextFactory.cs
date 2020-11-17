using Microsoft.EntityFrameworkCore.Design;
using Newtomsoft.Tools;

namespace Data
{
    public class DefaultDesignTimeDbContextFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args) => EntityFrameworkTools<Context>.CreateDbContext("Chromino");
    }
}