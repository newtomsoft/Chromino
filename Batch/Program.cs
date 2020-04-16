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
using System.Linq;

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
            Methods = new Dictionary<string, Action>
            {
                { "AddTips", AddTips },
                { "DeleteGuests", DeleteGuests },
                { "DeleteGamesWithoutPlayerTurn", DeleteGamesWithoutPlayerTurn },
                { "DeleteGamesWithOnlyBots", DeleteGamesWithOnlyBots },
                { "PlayBots", PlayBots },
                { "ClearTurnWhenGameFinish", ClearTurnWhenGameFinish},
                { "DeleteOldSinglesGames", DeleteOldSinglesGames},
            };

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
        }
        private static void DeleteGuests()
        {
            Console.WriteLine();
            Console.WriteLine("Suppression des invités et données associées");
            TimeSpan guestLifeTime = new TimeSpan(6, 0, 0);
            var guestsToDelete = PlayerDal.ListGuestWithOldGames(guestLifeTime, out var gamesIdToDelete);
            int nbDelete;
            if ((nbDelete = ChrominoInGameDal.Delete(gamesIdToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} ChrominosInGame ok");
            if ((nbDelete = ChrominoInHandDal.Delete(gamesIdToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} ChrominosInHand ok");
            if ((nbDelete = ChrominoInHandLastDal.Delete(gamesIdToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} ChrominosInHandLast ok");
            if ((nbDelete = GamePlayerDal.Delete(gamesIdToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} GamePlayer ok");
            if ((nbDelete = GoodPositionDal.Delete(gamesIdToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} GoodPosition et GoodPositionLevel ok");
            if ((nbDelete = SquareDal.Delete(gamesIdToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} Square ok");
            if ((nbDelete = GameDal.Delete(gamesIdToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} Game ok");
            if ((nbDelete = PlayerDal.Delete(guestsToDelete)) > 0)
                Console.WriteLine($"  Suppression de {nbDelete} invités ok");
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
                    case PlayReturn.GameFinish:
                        Console.WriteLine($"  la partie {gameId} est terminée");
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

        private static void ClearTurnWhenGameFinish()
        {
            Console.WriteLine();
            Console.WriteLine("erase des tours des joueurs pour parties terminées");
            var gamesPlayers = from gp in Ctx.GamesPlayers
                               join g in Ctx.Games on gp.GameId equals g.Id
                               where g.Status == GameStatus.Finished && gp.Turn
                               select gp;

            foreach (GamePlayer gamePlayer in gamesPlayers)
            {
                Console.WriteLine($"  partie {gamePlayer.GameId}");
                gamePlayer.Turn = false;
            }
            Ctx.SaveChanges();
        }

        private static void DeleteOldSinglesGames()
        {


        }

        private static void AddTips()
        {
            Console.WriteLine();
            Console.WriteLine("ajout des tips");

            Tip validateChromino = new Tip
            {
                TipName = TipName.HowValidateChromino,
                Description = "<p> Vous avez posé un chromino dans le jeu sans le valider.</p><p> Pensez à le valider en appuyant sur le bouton<button id='buttonInfoForPlaying' class='btn btn-toolbar btn-play' onclick='PlayChromino()'></button></p><p>Ou en appuyant sur le chromino jusqu'au flash.</p>",
            };

            Tip howMoveChromino = new Tip
            {
                TipName = TipName.HowMoveChromino,
                Description = "<p> Vous devez déplacer un chromino de votre main dans le jeu.</p><p>Avec la souris, cliquez sur un chromino et déplacer la tout en maintenant appuyer le bouton et relacher le à l'endroit voulu.</p><p>Ou avec un écran tactile, appuyer avec le doigt sur le chromino et glisser le.</p>",
            };

            List<Tip> tips = new List<Tip> { validateChromino, howMoveChromino };
            foreach (var tip in tips)
            {
                int tipId = (from t in Ctx.Tips
                             where t.TipName == tip.TipName
                             select t.Id).FirstOrDefault();
                if (tipId == 0)
                {
                    Ctx.Tips.Add(tip);
                    Ctx.SaveChanges();
                    Console.WriteLine($"  tip {tip.TipName} ajouté");
                }
                else
                {
                    Console.WriteLine($"  tip {tip.TipName} déjà présent en base");
                }
            }
        }
    }
}

