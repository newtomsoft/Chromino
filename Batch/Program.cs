using Data;
using Data.DAL;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**********************");
            Console.WriteLine("*** Batch Chromino ***");
            Console.WriteLine("**********************\n");
          
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env == null)
                env = "Development";
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT : {env}\n");
            string path = Directory.GetCurrentDirectory();
            IConfigurationBuilder builder = new ConfigurationBuilder()
                               .SetBasePath(path)
                               .AddJsonFile($"appsettings.{env}.json");
            IConfigurationRoot config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultContext");
            DbContextOptionsBuilder<Context> optionBuilder = new DbContextOptionsBuilder<Context>();
            optionBuilder.UseSqlServer(connectionString);
            Context context = new Context(optionBuilder.Options);
            GamePlayerDal gamePlayerDal = new GamePlayerDal(context);
            GameDal gameDal = new GameDal(context);

            #region jeuxEnCoursSansTourDeJoueur
            Console.WriteLine("Suppression des jeux en cours sans joueur dont c'est le tour");
            var gamesInProgress = gameDal.ListInProgress();
            List<int> badGamesId = new List<int>();
            foreach (Game game in gamesInProgress)
            {
                Player playerTurn = gamePlayerDal.PlayerTurn(game.Id);
                if (playerTurn == null)
                    badGamesId.Add(game.Id);
            }
            foreach (int id in badGamesId)
            {
                Console.WriteLine($"  Suppression du jeu {id}");
                context.GamesPlayers.RemoveRange(gamePlayerDal.List(id));
                context.Games.Remove(gameDal.Details(id));
            }
            context.SaveChanges();
            Console.WriteLine("\n");
            #endregion


        }
    }
}
