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
        private static ContactDal ContactDal;
        private static string Option;
        private static Dictionary<string, Action> Methods;

        static void Main(string[] args)
        {
            Methods = new Dictionary<string, Action>
            {
                //{ "CreateContacts", CreateContacts },
                { "AddBots", AddBots },
                { "AddTips", AddTips },
                { "AddPlayErrors", AddPlayErrors },
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
            Option = args.Length > 0 ? args[0] : string.Empty;
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
            ContactDal = new ContactDal(Ctx);
        }
        private static void AddBots()
        {
            Console.WriteLine();
            Console.WriteLine("Création des bots");
            List<Player> bots = new List<Player>();
            for (int i = 1; i <= 8; i++)
            {
                bots.Add(new Player { Bot = true, UserName = "Bot" + i });
            }
            foreach (var bot in bots)
            {
                int botId = (from b in Ctx.Players
                             where b.UserName == bot.UserName
                             select b.Id).FirstOrDefault();
                if (botId == 0)
                {
                    Ctx.Players.Add(bot);
                    Ctx.SaveChanges();
                    Console.WriteLine($"  bot {bot.UserName} ajouté");
                }
                else
                {
                    Console.WriteLine($"  (bot {bot.UserName} déjà présent en base)");
                }
            }
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
                playreturn = botBI.PlayBot(out ChrominoInGame _);
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
            #region tips
            Tip welcome = new Tip
            {
                Name = "Welcome",
                HeadPictureClass = "",
                Description = " <p>Bienvenue dans ce jeu.<p><p>Vous devez accoler vos chrominos contre d'autres en assurant au moins 2 contacts entre des carrés de mêmes couleurs.</p><p>En cliquant la 1ère fois sur un élement du jeu, une aide spécifique vous guidera.</p><p>Bons jeux !</p>",
                IllustrationPictureClass = "img-okko",
            };
            Tip validateChromino = new Tip
            {
                Name = "ValidateChromino",
                HeadPictureClass = "img-infogame",
                Description = "<p>Vous avez posé un chromino dans le jeu sans le valider.</p><p> Pensez à le valider en appuyant sur le bouton<button class='btn btn-toolbar img-play' onclick='PlayChromino()'></button></p><p>Ou en appuyant sur le chromino jusqu'au flash.</p>",
            };
            Tip home = new Tip
            {
                Name = "Home",
                HeadPictureClass = "img-home",
                Description = "<p>Cette icône permet de retourner à la page d’accueil.</p>",
            };
            Tip info = new Tip
            {
                Name = "Info",
                HeadPictureClass = "img-infogame",
                Description = "<p>Cette icône ouvre une fenêtre qui indique les informations essentielles de la partie : </p><li>L'ordre aléatoire des joueurs.</li><li>Le nombre de chrominos par joueur.</li><li>Le nombre de chrominos de la pioche.</li><br/><p>Les points autour de l’icône symbolisent les joueurs et leur statut (en ligne sur la partie, en ligne, hors ligne)</p><p>A noter que lorsqu’il ne reste qu’un chromino à un joueur, celui-ci sera visible dans cette fenêtre.</p>",
            };
            Tip help = new Tip
            {
                Name = "Help",
                HeadPictureClass = "img-helpPlay",
                Description = "<p>Cette icône permet de connaître les emplacements possibles des chrominos.</p><p>Ils sont symbolisés par une couleur.</p><p>Un numéro indique le nombre d'aides restant. </p><p>Ce nombre augmente de 3 par victoire, 2 par victoire ex-æquo, et d'1 par défaite.</p><p>A utiliser donc avec parcimonie.</p>",
            };
            Tip chat = new Tip
            {
                Name = "Chat",
                HeadPictureClass = "img-chat",
                Description = "<p>Cette icône ouvre une fenêtre de chat.</p><p>Un numéro indique la réception de nouveaux messages.</p>",
            };
            Tip memo = new Tip
            {
                Name = "Memo",
                HeadPictureClass = "img-memo",
                Description = "<p>Cette icône ouvre un bloc note permettant de noter les chrominos des adversaires pour les bloquer le moment venu.</p><p>Un numéro apparaît lorsqu'une ou plusieurs notes est inscrite dans celui-ci.</p>",
            };
            Tip next = new Tip
            {
                Name = "NextGame",
                HeadPictureClass = "img-nextgame",
                Description = "<p>Cette icône permet de passer à la partie suivante.</p><p>Idéal pour jouer plusieurs parties les unes après les autres sans repasser par l'accueil.</p>",
            };
            Tip draw = new Tip
            {
                Name = "DrawChromino",
                HeadPictureClass = "img-draw",
                Description = "<p>Cette icône permet de piocher lorsqu'on ne peut pas (ou qu'on ne veut pas) placer de chromino dans le jeu.</p>",
            };
            Tip skip = new Tip
            {
                Name = "SkipTurn",
                HeadPictureClass = "img-skip",
                Description = "<p>Cette icône permet de passer son tour si le chromino pioché ne peut pas être placé (ou qu'on ne veut pas le poser)</p>",
            };
            Tip handChrominos = new Tip
            {
                Name = "Hand",
                HeadPictureClass = "img-hand",
                Description = "<p>Pour remporter la partie, les chrominos de votre main doivent tous être posés dans le jeu à des positions valides.</p><p>Pour déplacer un chromino, appuyer dessus tout en maintenant la pression et relacher à l'endroit voulu.<p><p>Pour faire pivoter un chromino de 90°, appuyer brièvement dessus.<p>",
            };
            Tip history = new Tip
            {
                Name = "HistoryChrominos",
                HeadPictureClass = "img-previous-next",
                Description = "<p>Ces icônes permettent de voir l'enchaînement des chrominos posés dans le jeu.</p><p>Cela s'avère pratique lorsque l’on joue à plusieurs ou avec des pauses.</p>",
            };
            Tip play = new Tip
            {
                Name = "PlayChromino",
                HeadPictureClass = "img-play",
                Description = "<p>Cette icône permet de valider le chromino placé dans le jeu.</p><p>Il est également possible de le valider en appuyant sur celui-ci pendant une courte durée.</p>",
            };
            Tip cameleon = new Tip
            {
                Name = "Cameleon",
                HeadPictureClass = "img-cameleon",
                Description = "<p>Ce chromino \\\"caméléon\\\" a un joker en son centre.</p><p>Ce joker peut être accolé à n'importe quelle couleur d'un autre chromino comme sur l'exemple ci-dessous.</p>",
                IllustrationPictureClass = "img-playcameleon",
            };
            #endregion
            List<Tip> tips = new List<Tip> { home, info, help, chat, memo, next, draw, skip, handChrominos, validateChromino, history, play, cameleon, welcome };
            AddTipsInDb(tips);
        }
        private static void AddTipsInDb(List<Tip> tips)
        {
            foreach (var tip in tips)
            {
                var tipId = (from t in Ctx.Tips
                             where t.Name == tip.Name
                             select t.Id).FirstOrDefault();
                if (tipId == 0)
                {
                    Ctx.Tips.Add(tip);
                    Ctx.SaveChanges();
                    Console.WriteLine($"  tip {tip.Name} ajouté");
                }
                else
                {
                    Console.WriteLine($"  tip {tip.Name} déjà présent en base, MAJ...");
                    tip.Id = tipId;
                    Ctx.Tips.Update(tip);
                    int change = Ctx.SaveChanges();
                    if (change == 1)
                        Console.WriteLine($"  tip {tip.Name} modifié");
                    else
                        Console.WriteLine($"  (tip {tip.Name} inchangé)");
                }
            }
        }
        private static void AddPlayErrors()
        {
            Console.WriteLine();
            Console.WriteLine("ajout des erreurs de jeux");
            #region erreurs
            PlayError differentColorsAround = new PlayError
            {
                Name = "DifferentColorsAround",
                Description = "Les cotés de votre chromino ne peuvent pas toucher des couleurs différentes.",
                IllustrationPictureClass = "img-diffsides",
                IllustrationPictureCaption = "exemple invalide et exemple valide"
            };
            PlayError notFree = new PlayError
            {
                Name = "NotFree",
                Description = "L'emplacement où vous posez votre chromino n'est pas libre.",
            };
            PlayError notMinTwoSameColors = new PlayError
            {
                Name = "NotMinTwoSameColors",
                Description = "Au moins 2 cotés de votre chromino doivent toucher un chromino en jeu.",
                IllustrationPictureClass = "img-2sides",
                IllustrationPictureCaption = "exemples valides",
            };
            PlayError lastChrominoIsCameleon = new PlayError
            {
                Name = "LastChrominoIsCameleon",
                Description = "Vous ne pouvez pas jouer un chromino caméléon en dernier. Vous devez piocher",
                IllustrationPictureClass = "img-lastturncameleon",
                IllustrationPictureCaption = "exemple de chromino Caméléon"
            };
            PlayError notPlayerTurn = new PlayError
            {
                Name = "NotPlayerTurn",
                Description = "Ce n'est pas votre tour de jouer",
            };
            PlayError errorGameFinish = new PlayError
            {
                Name = "ErrorGameFinish",
                Description = "La partie est terminée.",
            };
            PlayError cantDraw2TimesInARow = new PlayError
            {
                Name = "CantDraw2TimesInARow",
                Description = "Vous ne pouvez pas piocher 2 fois de suite. Vous devez passer votre tour.",
            };
            PlayError noMoreChrominosInStack = new PlayError
            {
                Name = "NoMoreChrominosInStack",
                Description = "Il n'y a plus de chrominos dans la pioche. Vous devez soit poser un chromino, soit passer votre tour.",
            };
            #endregion
            List<PlayError> errors = new List<PlayError> { differentColorsAround, notFree, notMinTwoSameColors, lastChrominoIsCameleon, notPlayerTurn, errorGameFinish, cantDraw2TimesInARow, noMoreChrominosInStack };
            AddPlayErrorsInDb(errors);
        }
        private static void AddPlayErrorsInDb(List<PlayError> errors)
        {
            foreach (var error in errors)
            {
                int tipId = (from t in Ctx.PlayErrors
                             where t.Name == error.Name
                             select t.Id).FirstOrDefault();
                if (tipId == 0)
                {
                    Ctx.PlayErrors.Add(error);
                    Ctx.SaveChanges();
                    Console.WriteLine($"  erreur {error.Name} ajouté");
                }
                else
                {
                    Console.WriteLine($"  (erreur {error.Name} déjà présent en base)");
                }
            }
        }
        private static void CreateContacts()
        {
            Console.WriteLine();
            Console.WriteLine("ajout des contacts initiaux");
            List<Player> players = PlayerDal.List();
            foreach (var player in players)
            {
                List<Game> games = GamePlayerDal.Games(player.Id);
                foreach (var game in games)
                {
                    List<GamePlayer> gamePlayers = GamePlayerDal.GamePlayers(game.Id);
                    foreach (var gamePlayer in gamePlayers)
                    {
                        ContactDal.Add(player.Id, gamePlayer.PlayerId);
                    }
                }
            }
        }
    }
}