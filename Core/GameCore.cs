using Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool;

namespace Data.Core
{
    public partial class GameCore
    {
        /// <summary>
        /// nombre de chromino dans la main des joueurs en début de partie
        /// </summary>
        private const int BeginGameChrominoInHand = 8;

        /// <summary>
        /// DbContext du jeu
        /// </summary>
        private readonly Context Ctx;

        /// <summary>
        /// variables d'environnement
        /// </summary>
        private readonly IWebHostEnvironment Env;

        /// <summary>
        /// les différentes Dal utilisées du context 
        /// </summary>
        private readonly ChrominoInGameDal ChrominoInGameDal;
        private readonly ChrominoInHandDal ChrominoInHandDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;
        private readonly ComputedChrominosDal ComputedChrominosDal;

        /// <summary>
        /// Id du jeu
        /// </summary>
        private int GameId { get; set; }

        /// <summary>
        /// Liste des joueurs du jeu
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// List des GamePlayer du jeu (jointure entre Game et Player)
        /// </summary>
        public List<GamePlayer> GamePlayers { get; set; }

        private ComputedChrominoCore ComputedChrominoCore { get; set; }

        public GameCore(Context ctx, IWebHostEnvironment env, int gameId)
        {
            Env = env;
            Ctx = ctx;
            GameId = gameId;
            GameDal = new GameDal(ctx);
            ChrominoInGameDal = new ChrominoInGameDal(ctx);
            ChrominoInHandDal = new ChrominoInHandDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            ComputedChrominosDal = new ComputedChrominosDal(ctx);
            ComputedChrominoCore = new ComputedChrominoCore(ctx, GameId);
            Players = GamePlayerDal.Players(gameId);
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
            {
                GamePlayers.Add(GamePlayerDal.Details(gameId, player.Id));
            }
        }

        /// <summary>
        /// Commence une partie
        /// </summary>
        /// <param name="playerNumber"></param>
        public void BeginGame(int playerNumber)
        {
            if (playerNumber == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleInProgress);
            else
                GameDal.SetStatus(GameId, GameStatus.InProgress);
            ChrominoInGame chrominoInGame = ChrominoInGameDal.FirstToGame(GameId);
            Play(chrominoInGame);
            FillHand();
            new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            ChangePlayerTurn();
        }

        /// <summary>
        /// change le joueur dont c'est le tour de jouer
        /// </summary>
        public void ChangePlayerTurn()
        {
            if (GamePlayers.Count == 1)
            {
                GamePlayers[0].Turn = true;
            }
            else
            {
                GamePlayer gamePlayer = (from gp in GamePlayers
                                         where gp.Turn == true
                                         select gp).FirstOrDefault();

                if (gamePlayer == null)
                {
                    gamePlayer = (from gp in GamePlayers
                                  orderby gp.Id descending
                                  select gp).FirstOrDefault();
                }
                gamePlayer.PreviouslyDraw = false;
                bool selectNext = false;
                bool selected = false;
                foreach (GamePlayer currentGamePlayer in GamePlayers)
                {
                    if (selectNext)
                    {
                        currentGamePlayer.Turn = true;
                        selected = true;
                        break;
                    }
                    if (gamePlayer.Id == currentGamePlayer.Id)
                        selectNext = true;
                }
                if (!selected)
                    GamePlayers[0].Turn = true;

                gamePlayer.Turn = false;
            }
            GameDal.UpdateDate(GameId);
            Ctx.SaveChanges();
        }

        /// <summary>
        /// Passe le tour
        /// Indique que la partie est terminée si c'est au dernier joueur du tour de jouer, 
        /// Indique que la partie est terminée s'il n'y a plus de chromino dans la pioche et que tous les joueurs ont passé
        /// </summary>
        /// <param name="playerId"></param>
        public PlayReturn SkipTurn(int playerId)
        {
            GamePlayerDal.SetPass(GameId, playerId, true);
            if (ChrominoInGameDal.InStack(GameId) == 0 && GamePlayerDal.IsAllPass(GameId) || (IsRoundLastPlayer(playerId) && GamePlayerDal.IsSomePlayerWon(GameId)))
                SetGameFinished();
            ChangePlayerTurn();
            return PlayReturn.SkipTurn;
        }

        /// <summary>
        /// rempli la main du ou de tous les joueurs, de chrominos
        /// </summary>
        /// <param name="gamePlayer">le joueur concerné. Tous les joueurs si null</param>
        private void FillHand(GamePlayer gamePlayer = null)
        {
            if (gamePlayer == null)
            {
                foreach (GamePlayer currentGamePlayer in GamePlayers)
                    FillHand(currentGamePlayer);

                ComputedChrominoCore.UpdateAllPlayersWholeGame();
            }
            else
            {
                for (int i = 0; i < BeginGameChrominoInHand; i++)
                    ChrominoInHandDal.FromStack(GameId, gamePlayer.PlayerId);
            }
        }

        /// <summary>
        /// marque la partie terminée
        /// </summary>
        private void SetGameFinished()
        {
            if (GamePlayerDal.PlayersNumber(GameId) == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleFinished);
            else
                GameDal.SetStatus(GameId, GameStatus.Finished);
        }

        /// <summary>
        /// return true si l'id du joueur est l'id du dernier joueur dans le tour
        /// </summary>
        /// <param name="playerId">id du joueur courant</param>
        /// <returns></returns>
        private bool IsRoundLastPlayer(int playerId)
        {
            int roundLastPlayerId = GamePlayerDal.RoundLastPlayerId(GameId);
            return playerId == roundLastPlayerId ? true : false;
        }

        /// <summary>
        /// indique si les autres joueurs à suivre dans le tour peuvent potentiellement gagner
        /// </summary>
        /// <param name="playerId">Id du joueur à partir duquel les autres joueurs sont déterminés</param>
        /// <returns></returns>
        private bool IsNextPlayersCanWin(int playerId)
        {
            foreach (int currentPlayerId in GamePlayerDal.NextPlayersId(GameId, playerId))
            {
                if (ChrominoInHandDal.ChrominosNumber(GameId, currentPlayerId) == 1)
                    return true;
            }
            return false;
        }

        private List<Square> ComputeSquares(ChrominoInGame chrominoIG)
        {
            int gameId = chrominoIG.GameId;
            int xOrigin = chrominoIG.XPosition;
            int yOrigin = chrominoIG.YPosition;
            Chromino chromino = ChrominoDal.Details(chrominoIG.ChrominoId);
            List<Square> squares = new List<Square>() { new Square { GameId = gameId, X = xOrigin, Y = yOrigin, Color = chromino.FirstColor } };
            switch (chrominoIG.Orientation)
            {
                case Orientation.Horizontal:
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 1, Y = yOrigin, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 2, Y = yOrigin, Color = chromino.ThirdColor });
                    break;
                case Orientation.Vertical:
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 1, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 2, Color = chromino.ThirdColor });
                    break;
            }
            return squares;
        }

        public GameVM GameViewModel(int gameId, int playerId)
        {
            Player playerTurn = GamePlayerDal.PlayerTurn(gameId);
            List<Player> players = GamePlayerDal.Players(gameId);
            if (players.Where(x => x.Id == playerId).FirstOrDefault() != null || GamePlayerDal.IsBots(gameId))
            {
                // je joueur est bien dans la partie ou c'est une partie entre bots
                int chrominosInStackNumber = ChrominoInGameDal.InStack(gameId);
                string playerPseudo = PlayerDal.Pseudo(playerId);
                Dictionary<string, int> pseudosChrominos = new Dictionary<string, int>();
                List<string> pseudos = new List<string>();
                foreach (Player player in players)
                {
                    pseudosChrominos.Add(player.UserName, ChrominoInHandDal.ChrominosNumber(gameId, player.Id));
                    pseudos.Add(player.UserName);
                }
                Dictionary<string, Chromino> pseudos_lastChrominos = new Dictionary<string, Chromino>();
                foreach (var pseudo_chromino in pseudosChrominos)
                {
                    if (pseudo_chromino.Value == 1)
                        pseudos_lastChrominos.Add(pseudo_chromino.Key, ChrominoInHandDal.FirstChromino(gameId, GamePlayerDal.PlayerId(gameId, pseudo_chromino.Key)));
                }
                List<Chromino> identifiedPlayerChrominos;
                if (GamePlayerDal.IsBots(gameId)) // s'il n'y a que des bots en jeu, on regarde la partie et leur main
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(gameId, playerTurn.Id);
                else
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(gameId, playerId);
                if (GameDal.IsFinished(gameId) && !GamePlayerDal.GetViewFinished(gameId, playerId))
                {
                    GamePlayerDal.SetViewFinished(gameId, playerId);
                    if (GamePlayerDal.GetWon(gameId, playerId) == null)
                        GamePlayerDal.SetWon(gameId, playerId, false);
                }
                GamePlayer gamePlayerTurn = GamePlayerDal.Details(gameId, playerTurn.Id);
                GamePlayer gamePlayerIdentified = GamePlayerDal.Details(gameId, playerId);
                List<Square> squares = SquareDal.List(gameId);
                List<int> botsId = PlayerDal.BotsId();
                bool noTips = PlayerDal.Details(playerId).NoTips;
                List<ChrominoInGame> chrominosInGamePlayed = ChrominoInGameDal.List(gameId);
                Game game = GameDal.Details(gameId);
                bool opponenentsAreBots = GamePlayerDal.IsOpponenentsAreBots(gameId, playerId);
                GameVM gameViewModel = new GameVM(game, playerPseudo, playerId, squares, chrominosInStackNumber, pseudosChrominos, identifiedPlayerChrominos, playerTurn, gamePlayerTurn, gamePlayerIdentified, botsId, pseudos_lastChrominos, chrominosInGamePlayed, pseudos, opponenentsAreBots, noTips);
                return gameViewModel;
            }
            else return null;
        }
    }
}