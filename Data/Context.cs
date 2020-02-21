using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using System;
using System.Linq;

namespace Data
{
    public class Context : DbContext
    {
        public string ConnectionString { get; set; }
        public DbSet<Chromino> Chrominos { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Square> Squares { get; set; }
        public DbSet<GamePlayer> GamesPlayers { get; set; }
        public DbSet<ChrominoInHand> ChrominosInHand { get; set; }
        public DbSet<ChrominoInGame> ChrominosInGame { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>()
            .HasIndex(e => e.Pseudo)
            .HasName("NIX_PlayerPseudo")
            .IsUnique();
        }
    }
}
