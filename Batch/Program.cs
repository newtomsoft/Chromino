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
using System.Threading.Tasks;

namespace Batch
{
    class Program
    {
        private static Context Ctx;
        private static GamePlayerDal GamePlayerDal;
        private static GameDal GameDal;
        private static PlayerDal PlayerDal;
        private static ChrominoInGameDal ChrominoInGameDal;
        private static ChrominoInHandDal ChrominoInHandDal;
        private static ChrominoInHandLastDal ChrominoInHandLastDal;
        private static GoodPositionDal GoodPositionDal;
        private static SquareDal SquareDal;
        private static string Option;
        private static Dictionary<string, Action> Methods;

        static void Main(string[] args)
        {
            Init(args);

            if (Option == "")
                foreach (KeyValuePair<string, Action> key_value in Methods)
                    key_value.Value();
            else if (int.TryParse(Option, out int result))
                DeleteGame(result);
            else if (Methods.ContainsKey(Option))
                Methods[Option]();
            else
                Console.WriteLine("paramètre invalide");

        }

        private static void Init(string[] args)
        {
            Option = args.Length > 0 ? args[0] : String.Empty;
            Console.WriteLine("**************************");
            Console.WriteLine("***** Batch Chromino *****");
            Console.WriteLine($"* le {DateTime.Now.ToString("dd/MM/yyyy à HH:mm").Replace(':', 'h')}  *");
            Console.WriteLine("**************************");
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
            GamePlayerDal = new GamePlayerDal(Ctx);
            GameDal = new GameDal(Ctx);
            PlayerDal = new PlayerDal(Ctx);
            ChrominoInGameDal = new ChrominoInGameDal(Ctx);
            ChrominoInHandDal = new ChrominoInHandDal(Ctx);
            ChrominoInHandLastDal = new ChrominoInHandLastDal(Ctx);
            GoodPositionDal = new GoodPositionDal(Ctx);
            SquareDal = new SquareDal(Ctx);
            Methods = new Dictionary<string, Action>
            {
                { "DeleteGuests", DeleteGuests },
                { "DeleteGamesWithoutPlayerTurn", DeleteGamesWithoutPlayerTurn },
                { "DeleteGamesWithOnlyBots", DeleteGamesWithOnlyBots },
                { "PlayBots", PlayBots }
            };
        }
        private static void DeleteGuests()
        {
            Console.WriteLine();
            Console.WriteLine("Suppression des invités et données associées");
            TimeSpan guestLifeTime = new TimeSpan(6, 0, 0);
            var guestsToDelete = PlayerDal.ListGuestWithOldGames(guestLifeTime, out var gamesIdToDelete);
            Console.WriteLine($"  Suppression de {ChrominoInGameDal.Delete(gamesIdToDelete)} ChrominosInGame ok");
            Console.WriteLine($"  Suppression de {ChrominoInHandDal.Delete(gamesIdToDelete)} ChrominosInHand ok");
            Console.WriteLine($"  Suppression de {ChrominoInHandLastDal.Delete(gamesIdToDelete)} ChrominosInHandLast ok");
            Console.WriteLine($"  Suppression de {GamePlayerDal.Delete(gamesIdToDelete)} GamePlayer ok");
            Console.WriteLine($"  Suppression de {GoodPositionDal.Delete(gamesIdToDelete)} GoodPosition et GoodPositionLevel ok");
            Console.WriteLine($"  Suppression de {SquareDal.Delete(gamesIdToDelete)} Square ok");
            Console.WriteLine($"  Suppression de {GameDal.Delete(gamesIdToDelete)} Game ok");
            Console.WriteLine($"  Suppression de {PlayerDal.Delete(guestsToDelete)} invités ok");
        }
        private static void DeleteGamesWithoutPlayerTurn()
        {
            Console.WriteLine();
            Console.WriteLine("Suppression des jeux en cours sans joueur dont c'est le tour");
            var gamesInProgress = GameDal.ListInProgress();
            List<int> badGamesId = new List<int>();
            foreach (Game game in gamesInProgress)
            {
                Player playerTurn = GamePlayerDal.PlayerTurn(game.Id);
                if (playerTurn == null)
                    badGamesId.Add(game.Id);
            }
            DeleteGames(badGamesId);
        }
        private static void DeleteGames(List<int> gamesId)
        {
            foreach (int id in gamesId)
                DeleteGame(id);
        }
        private static void DeleteGame(int id)
        {
            Console.WriteLine($"  Suppression du jeu {id}");
            Ctx.GamesPlayers.RemoveRange(GamePlayerDal.GamePlayers(id));
            Ctx.Games.Remove(GameDal.Details(id));
            Ctx.SaveChanges();
        }
        private static void DeleteGamesWithOnlyBots()
        {
            Console.WriteLine();
            Console.WriteLine("Suppresion des parties terminées avec que des bots");
            foreach (Game game in GameDal.List())
            {
                if (GamePlayerDal.IsAllBots(game.Id) && GameDal.IsFinished(game.Id))
                    DeleteGame(game.Id);
            }
        }
        private static void PlayBots()
        {
            Console.WriteLine();
            Console.WriteLine("Force les bots à jouer");
            List<GamePlayer> gameBots = GamePlayerDal.ListBotsTurn();
            foreach (GamePlayer gameBot in gameBots)
            {
                int playerId = gameBot.PlayerId;
                int gameId = gameBot.GameId;
                bool isNextBot = true;
                bool gameFinish = false;
                while (isNextBot && !gameFinish)
                {
                    gameFinish = PlayBot(gameId, playerId);
                    Player nextPlayer = GamePlayerDal.PlayerTurn(gameId);
                    playerId = nextPlayer.Id;
                    isNextBot = nextPlayer.Bot;
                }
            }
        }
        private static bool PlayBot(int gameId, int botId)
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
            while (playreturn.IsError() || playreturn == PlayReturn.DrawChromino || playreturn == PlayReturn.SkipTurn);
            if (playreturn == PlayReturn.GameFinish)
            {
                new GameBI(Ctx, null, gameId).SetGameFinished();
                Console.WriteLine($"  le bot a gagné la partie qui est terminée");
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
