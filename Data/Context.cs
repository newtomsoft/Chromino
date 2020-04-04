using Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class Context : IdentityDbContext<Player, Role, int>
    {
        public DbSet<Chromino> Chrominos { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Square> Squares { get; set; }
        public DbSet<GamePlayer> GamesPlayers { get; set; }
        public DbSet<ChrominoInHand> ChrominosInHand { get; set; }
        public DbSet<ChrominoInHandLast> ChrominosInHandLast { get; set; }
        public DbSet<ChrominoInGame> ChrominosInGame { get; set; }
        public DbSet<GoodPosition> ComputedChrominos { get; set; }
        public DbSet<GoodPositionLevel> ComputedChrominosOk { get; set; }


        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
