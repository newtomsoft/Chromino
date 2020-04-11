using ChrominoBI;
using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
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
        private static Context Ctx;

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
            Ctx = new Context(optionBuilder.Options);
            GamePlayerDal gamePlayerDal = new GamePlayerDal(Ctx);
            GameDal gameDal = new GameDal(Ctx);
            PlayerDal playerDal = new PlayerDal(Ctx);
            ChrominoInGameDal chrominoInGameDal = new ChrominoInGameDal(Ctx);
            ChrominoInHandDal chrominoInHandDal = new ChrominoInHandDal(Ctx);
            ChrominoInHandLastDal chrominoInHandLastDal = new ChrominoInHandLastDal(Ctx);
            GoodPositionDal goodPositionDal = new GoodPositionDal(Ctx);
            SquareDal squareDal = new SquareDal(Ctx);
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
                Ctx.GamesPlayers.RemoveRange(gamePlayerDal.List(id));
                Ctx.Games.Remove(gameDal.Details(id));
            }
            Ctx.SaveChanges();
            Console.WriteLine("\n");
            #endregion

            #region force a faire jouer les bots
            Console.WriteLine("Force les bots à jouer");
            List<GamePlayer> gameBots = gamePlayerDal.ListBotsTurn();
            foreach (GamePlayer gameBot in gameBots)
            {
                int playerId = gameBot.PlayerId;
                int gameId = gameBot.GameId;
                bool isNextBot = true;
                while (isNextBot)
                {
                    PlayBot(gameId, playerId);
                    Player nextPlayer = gamePlayerDal.PlayerTurn(gameId);
                    playerId = nextPlayer.Id;
                    isNextBot = nextPlayer.Bot;
                }       
            }
            #endregion

        }

        private static void PlayBot(int gameId, int botId)
        {
            BotBI botBI = new BotBI(Ctx, null, gameId, botId);
            PlayReturn playreturn;
            do
            {
                playreturn = botBI.PlayBot();
                switch (playreturn)
                {
                    case PlayReturn.Ok:
                        Console.WriteLine($"  le bot {botId} de la partie {gameId} a posé");
                        break;
                    case PlayReturn.DrawChromino:
                        Console.WriteLine($"  le bot {botId} de la partie {gameId} a pioché");
                        break;
                    case PlayReturn.SkipTurn:
                        Console.WriteLine($"  le bot {botId} de la partie {gameId} a passé");
                        break;
                }
            }
            while (playreturn.IsError() || playreturn == PlayReturn.DrawChromino);
            if (playreturn == PlayReturn.GameFinish)
            {
                new GameBI(Ctx, null, gameId).SetGameFinished();
                Console.WriteLine($"  le bot a gagné la partie qui est terminée");
            }
        }
    }
}
