using Core;
using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool;

namespace ChrominoBI
{
    public class PlayerBI
    {
        #region Properties
        /// <summary>
        /// DbContext du jeu
        /// </summary>
        protected readonly Context Ctx;

        /// <summary>
        /// variables d'environnement
        /// </summary>
        protected readonly IWebHostEnvironment Env;

        /// <summary>
        /// les différentes Dal utilisées du context 
        /// </summary>
        protected readonly ChrominoInGameDal ChrominoInGameDal;
        protected readonly ChrominoInHandDal ChrominoInHandDal;
        protected readonly ChrominoDal ChrominoDal;
        protected readonly SquareDal SquareDal;
        protected readonly PlayerDal PlayerDal;
        protected readonly GamePlayerDal GamePlayerDal;
        protected readonly GameDal GameDal;
        protected readonly GoodPositionDal GoodPositionDal;
        protected readonly ChrominoInHandLastDal ChrominoInHandLastDal;

        protected GoodPositionBI GoodPositionBI { get; set; }

        /// <summary>
        /// Id du jeu
        /// </summary>
        protected int GameId { get; set; }

        /// <summary>
        /// Id du joueur
        /// </summary>
        protected int PlayerId { get; set; }
        #endregion

        public PlayerBI(Context ctx, IWebHostEnvironment env, int gameId, int playerId)
        {
            Env = env;
            Ctx = ctx;
            GameId = gameId;
            PlayerId = playerId;
            GameDal = new GameDal(ctx);
            ChrominoInGameDal = new ChrominoInGameDal(ctx);
            ChrominoInHandDal = new ChrominoInHandDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            GoodPositionDal = new GoodPositionDal(ctx);
            ChrominoInHandLastDal = new ChrominoInHandLastDal(ctx);
            GoodPositionBI = new GoodPositionBI(ctx, GameId);
        }

        /// <summary>
        /// Passe le tour
        /// Indique que la partie est terminée si c'est au dernier joueur du tour de jouer, 
        /// Indique que la partie est terminée s'il n'y a plus de chromino dans la pioche et que tous les joueurs ont passé
        /// </summary>
        public int SkipTurn()
        {
            GamePlayerDal.SetSkip(GameId, PlayerId, true);
            if (ChrominoInGameDal.InStack(GameId) == 0 && GamePlayerDal.IsAllSkip(GameId) || (GamePlayerDal.IsSomePlayerWon(GameId) && (IsRoundLastPlayer() || !IsNextPlayersCanWin())))
            {
                new GameBI(Ctx, Env, GameId).SetGameFinished();
                List<int> playersIdWin = ChrominoInHandDal.PlayersIdWithMinChrominos(GameId);
                List<int> playersIdLoose = GamePlayerDal.PlayersId(GameId).Except(playersIdWin).ToList();
                foreach (int playerId in playersIdWin)
                    GamePlayerDal.SetWon(GameId, playerId);
                foreach (int playerId in playersIdLoose)
                    GamePlayerDal.SetWon(GameId, playerId, false);
            }
            return ChangePlayerTurn();
        }

        /// <summary>
        /// Tente de piocher un chromino et indique que le joueur à pioché au coup d'avant.
        /// S'il n'y a plus de chrimino dans la pioche, ne fait rien
        /// </summary>
        /// <param name="playReturn">PlayReturn.DrawChromino si pioche ok</param>
        /// <returns>id du chromino si tentative ok. sinon 0</returns>
        public int TryDrawChromino(out PlayReturn playReturn)
        {
            int chrominoId = ChrominoInHandDal.FromStack(GameId, PlayerId);
            if (chrominoId != 0)
            {
                GoodPositionBI.Add(PlayerId, chrominoId);
                GamePlayerDal.SetPreviouslyDraw(GameId, PlayerId);
                playReturn = PlayReturn.DrawChromino;
            }
            else
                playReturn = PlayReturn.NoMoreChrominosInStack;

            return chrominoId;
        }

        /// <summary>
        /// Tente de jouer chrominoInGame
        /// </summary>
        /// <param name="chrominoInGame">Chromino à jouer avec positions et orientation</param>
        /// <returns>PlayReturn.Ok si valide, PlayReturn.GameFinish si la partie est finie</returns>
        public PlayReturn Play(ChrominoInGame chrominoInGame)
        {
            int numberInHand = PlayerId == 0 ? 0 : ChrominoInHandDal.ChrominosNumber(GameId, PlayerId);
            List<Square> squaresInGame = SquareDal.List(GameId);
            Chromino chromino = ChrominoDal.Details(chrominoInGame.ChrominoId);
            ChrominoInGameBI chrominoInGameBI = new ChrominoInGameBI(Ctx, GameId, chrominoInGame);
            List<Coordinate> chrominoCoordinates = chrominoInGameBI.ChrominoCoordinates();
            if (!chrominoInGameBI.IsValidChriminoInGame(ref squaresInGame, out PlayReturn errorPlayReturn))
                return errorPlayReturn;

            bool firstSquareOpenRight = false, firstSquareOpenBottom = false, firstSquareOpenLeft = false, firstSquareOpenTop = false;
            bool secondSquareOpenRight = false, secondSquareOpenBottom = false, secondSquareOpenLeft = false, secondSquareOpenTop = false;
            bool thirdSquareOpenRight = false, thirdSquareOpenBottom = false, thirdSquareOpenLeft = false, thirdSquareOpenTop = false;

            if (chrominoInGame.Orientation == Orientation.Horizontal)
            {
                firstSquareOpenRight = true;
                secondSquareOpenRight = true;
                secondSquareOpenLeft = true;
                thirdSquareOpenLeft = true;
            }
            else
            {
                firstSquareOpenBottom = true;
                secondSquareOpenTop = true;
                secondSquareOpenBottom = true;
                thirdSquareOpenTop = true;
            }

            List<Square> squares = new List<Square>
            {
                new Square { GameId = GameId, X = chrominoCoordinates[0].X, Y = chrominoCoordinates[0].Y, Color = chrominoInGame.Flip ? chromino.ThirdColor : chromino.FirstColor, OpenRight = firstSquareOpenRight, OpenBottom = firstSquareOpenBottom, OpenLeft = firstSquareOpenLeft, OpenTop = firstSquareOpenTop },
                new Square { GameId = GameId, X = chrominoCoordinates[1].X, Y = chrominoCoordinates[1].Y, Color = chromino.SecondColor, OpenRight = secondSquareOpenRight, OpenBottom = secondSquareOpenBottom, OpenLeft = secondSquareOpenLeft, OpenTop = secondSquareOpenTop },
                new Square { GameId = GameId, X = chrominoCoordinates[2].X, Y = chrominoCoordinates[2].Y, Color = chrominoInGame.Flip ? chromino.FirstColor : chromino.ThirdColor, OpenRight = thirdSquareOpenRight, OpenBottom = thirdSquareOpenBottom, OpenLeft = thirdSquareOpenLeft, OpenTop = thirdSquareOpenTop }
            };

            SquareDal.Add(squares);
            byte move = GameDal.Details(GameId).Move;
            chrominoInGame.Move = move;
            ChrominoInGameDal.Add(chrominoInGame);
            ChrominoInHandDal.DeleteInHand(GameId, chromino.Id);
            GameDal.IncreaseMove(GameId);
            GoodPositionBI.RemovePlayedChromino(PlayerId, chrominoInGame.ChrominoId);
            GoodPositionBI.RemoveAndUpdateAllPlayers();

            PlayReturn playreturn = PlayReturn.Ok;
            if (PlayerId != 0)
            {
                numberInHand--;
                GamePlayerDal.SetSkip(GameId, PlayerId, false);
                int points = ChrominoDal.Details(chrominoInGame.ChrominoId).Points;
                GamePlayerDal.AddPoints(GameId, PlayerId, points);
                if (GamePlayerDal.Players(GameId).Count > 1)
                    PlayerDal.AddPoints(PlayerId, points);

                if (numberInHand == 1)
                {
                    ChrominoInHandLastDal.Update(GameId, PlayerId, ChrominoInHandDal.FirstChromino(GameId, PlayerId).Id);
                }
                else if (numberInHand == 0)
                {
                    ChrominoInHandLastDal.Delete(GameId, PlayerId);
                    GamePlayerDal.SetWon(GameId, PlayerId);
                    if (GamePlayerDal.PlayersNumber(GameId) == 1)
                    {
                        int chrominosInGame = ChrominoInGameDal.Count(GameId);
                        PlayerDal.Details(PlayerId).SinglePlayerGamesPoints += chrominosInGame switch
                        {
                            8 => 100,
                            9 => 90,
                            10 => 85,
                            11 => 82,
                            _ => 92 - chrominosInGame,
                        };
                        PlayerDal.Details(PlayerId).SinglePlayerGamesFinished++;
                        Ctx.SaveChanges();
                    }
                    if (IsRoundLastPlayer() || !IsNextPlayersCanWin())
                    {
                        new GameBI(Ctx, Env, GameId).SetGameFinished();
                        playreturn = PlayReturn.GameFinish;
                    }
                }
                else
                {
                    int chrominoId = ChrominoInHandLastDal.IdOf(GameId, PlayerId);
                    if (chrominoInGame.ChrominoId == chrominoId)
                        ChrominoInHandLastDal.Delete(GameId, PlayerId);
                }

                if (IsRoundLastPlayer() && GamePlayerDal.IsSomePlayerWon(GameId))
                {
                    new GameBI(Ctx, Env, GameId).SetGameFinished();
                    GamePlayerDal.SetViewFinished(GameId, PlayerId);
                    playreturn = PlayReturn.GameFinish;
                }

                if (Env != null)
                    new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
                ChangePlayerTurn();
            }
            return playreturn;
        }

        /// <summary>
        /// Change le joueur dont c'est le tour de jouer.
        /// Si la partie est finie, ce n'est plus le tour d'aucun joueur
        /// </summary>
        /// <returns>id du joueur dont c'est le tour. 0 si la partie est finie</returns>
        public int ChangePlayerTurn()
        {
            List<Player> players = GamePlayerDal.Players(GameId);
            int playerTurnId = PlayerId;
            List<GamePlayer> gamePlayers = new List<GamePlayer>();
            foreach (Player player in players)
                gamePlayers.Add(GamePlayerDal.Details(GameId, player.Id));

            if (gamePlayers.Count != 1)
            {

                GamePlayer gamePlayer = (from gp in gamePlayers
                                         where gp.Turn == true
                                         select gp).FirstOrDefault();

                if (gamePlayer == null)
                {
                    gamePlayer = (from gp in gamePlayers
                                  orderby gp.Id descending
                                  select gp).FirstOrDefault();
                }
                if (GameDal.IsFinished(GameId))
                {
                    gamePlayer.Turn = false;
                    playerTurnId = 0;
                }
                else
                {
                    gamePlayer.PreviouslyDraw = false;
                    bool selectNext = false;
                    bool selected = false;
                    foreach (GamePlayer currentGamePlayer in gamePlayers)
                    {
                        if (selectNext)
                        {
                            currentGamePlayer.Turn = true;
                            playerTurnId = currentGamePlayer.PlayerId;
                            selected = true;
                            break;
                        }
                        if (gamePlayer.Id == currentGamePlayer.Id)
                            selectNext = true;
                    }
                    if (!selected)
                    {
                        gamePlayers[0].Turn = true;
                        playerTurnId = gamePlayers[0].PlayerId;
                    }
                    gamePlayer.Turn = false;
                }
            }
            else
            {
                gamePlayers[0].Turn = true;
                playerTurnId = gamePlayers[0].PlayerId;
            }
            GameDal.UpdateDate(GameId);
            Ctx.SaveChanges();
            return playerTurnId;
        }

        public void LooseGame(int playerId)
        {
            GamePlayerDal.SetViewFinished(GameId, playerId);
            PlayerDal.IncreaseHelp(playerId, 1);
            if (GamePlayerDal.GetWon(GameId, playerId) == null)
                GamePlayerDal.SetWon(GameId, playerId, false);
        }

        public bool WinGame(int playerId, bool drawGame = false)
        {
            GamePlayerDal.SetViewFinished(GameId, playerId);
            GamePlayerDal.SetWon(GameId, playerId);
            PlayerDal.IncreaseWinAndHelp(playerId, drawGame ? 2 : 3);
            return true;
        }

        /// <summary>
        /// indique si les autres joueurs à suivre dans le tour peuvent potentiellement gagner
        /// </summary>
        /// <returns></returns>
        private bool IsNextPlayersCanWin()
        {
            foreach (int playerId in GamePlayerDal.NextPlayersId(GameId, PlayerId))
                if (ChrominoInHandDal.ChrominosNumber(GameId, playerId) == 1)
                    return true;
            return false;
        }

        /// <summary>
        /// return true si l'id du joueur est l'id du dernier joueur dans le tour
        /// </summary>
        /// <param name="playerId">id du joueur courant</param>
        /// <returns></returns>
        protected bool IsRoundLastPlayer()
        {
            int roundLastPlayerId = GamePlayerDal.LastPlayerIdInRound(GameId);
            return PlayerId == roundLastPlayerId ? true : false;
        }
    }
}
