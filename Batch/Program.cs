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
            Console.WriteLine("**************************");
            Console.WriteLine("***** Batch Chromino *****");
            Console.WriteLine($"* le {DateTime.Now.ToString("dd/MM/yyyy à HH:mm").Replace(':', 'h')}  *");
            Console.WriteLine("**************************");
            Console.WriteLine();
            #region setting
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env == null)
                env = "Development";
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT : {env}\n");
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
            PlayerDal playerDal = new PlayerDal(context);
            ChrominoInGameDal chrominoInGameDal = new ChrominoInGameDal(context);
            ChrominoInHandDal chrominoInHandDal = new ChrominoInHandDal(context);
            ChrominoInHandLastDal chrominoInHandLastDal = new ChrominoInHandLastDal(context);
            GoodPositionDal goodPositionDal = new GoodPositionDal(context);
            SquareDal squareDal = new SquareDal(context);
            TimeSpan guestLifeTime = new TimeSpan(6, 0, 0);
            #endregion

            #region suppression invités
            Console.WriteLine("Suppression des invités et données associées");
            var guestsToDelete = playerDal.ListGuestWithOldGames(guestLifeTime, out var gamesIdToDelete);
            Console.WriteLine($"  Suppression de {chrominoInGameDal.Delete(gamesIdToDelete)} ChrominosInGame ok");
            Console.WriteLine($"  Suppression de {chrominoInHandDal.Delete(gamesIdToDelete)} ChrominosInHand ok");
            Console.WriteLine($"  Suppression de {chrominoInHandLastDal.Delete(gamesIdToDelete)} ChrominosInHandLast ok");
            Console.WriteLine($"  Suppression de {gamePlayerDal.Delete(gamesIdToDelete)} GamePlayer ok");
            Console.WriteLine($"  Suppression de {goodPositionDal.Delete(gamesIdToDelete)} GoodPosition et GoodPositionLevel ok");
            Console.WriteLine($"  Suppression de {squareDal.Delete(gamesIdToDelete)} Square ok");
            Console.WriteLine($"  Suppression de {gameDal.Delete(gamesIdToDelete)} Game ok");
            Console.WriteLine($"  Suppression de {playerDal.Delete(guestsToDelete)} invités ok");
            #endregion

            #region suppression jeux en Cours sans tour de joueur
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
