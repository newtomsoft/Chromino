using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class Context : IdentityDbContext<Player, IdentityRole<int>, int>
    {
        public DbSet<Chromino> Chrominos { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Square> Squares { get; set; }
        public DbSet<GamePlayer> GamesPlayers { get; set; }
        public DbSet<ChrominoInHand> ChrominosInHand { get; set; }
        public DbSet<ChrominoInHandLast> ChrominosInHandLast { get; set; }
        public DbSet<ChrominoInGame> ChrominosInGame { get; set; }
        public DbSet<GoodPosition> GoodPositions { get; set; }
        public DbSet<GoodPositionLevel> GoodPositionsLevel { get; set; }
        public DbSet<Tip> Tips { get; set; }
        public DbSet<TipOff> TipsOff { get; set; }
        public DbSet<PlayError> PlayErrors { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<PrivateMessage> PrivatesMessages { get; set; }
        public DbSet<PrivateMessageLatestRead> PrivatesMessagesLatestRead { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
