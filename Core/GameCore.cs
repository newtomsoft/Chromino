using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Microsoft.AspNetCore.Hosting;
using System;
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
        private readonly GameChrominoDal GameChrominoDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;

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

        public GameCore(Context ctx, IWebHostEnvironment env, int gameId)
        {
            Env = env;
            Ctx = ctx;
            GameId = gameId;
            GameDal = new GameDal(ctx);
            GameChrominoDal = new GameChrominoDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            Players = GamePlayerDal.Players(gameId);
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
            {
                GamePlayers.Add(GamePlayerDal.Details(gameId, player.Id));
            }
        }

        /// <summary>
        /// Commence une partie seul ou à plusieurs
        /// </summary>
        /// <param name="playerNumber"></param>
        public void BeginGame(int playerNumber)
        {
            if (playerNumber == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleInProgress);
            else
                GameDal.SetStatus(GameId, GameStatus.InProgress);
            ChrominoInGame chrominoInGame = GameChrominoDal.FirstRandomToGame(GameId);
            PlayChromino(chrominoInGame, null);

            new PictureFactory(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            FillHandPlayers();
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
        public void SkipTurn(int playerId)
        {
            GamePlayerDal.SetPass(GameId, playerId, true);
            if (GameChrominoDal.InStack(GameId) == 0 && GamePlayerDal.IsAllPass(GameId) || (IsRoundLastPlayer(playerId) && GamePlayerDal.IsSomePlayerWon(GameId)))
                SetGameFinished();
            ChangePlayerTurn();
        }

        /// <summary>
        /// pioche un chromino et indique que le joueur à pioché au coup d'avant
        /// s'il n'y a plus de chrimino dans la pioche, passe le tour
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        public void DrawChromino(int playerId)
        {
            if (GameChrominoDal.StackToHand(GameId, playerId) == 0)
                SkipTurn(playerId);
            else
                GamePlayerDal.SetPreviouslyDraw(GameId, playerId);
        }

        /// <summary>
        /// retourne la couleur possible à cet endroit
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="adjacentChrominos"></param>
        /// <returns>Cameleon si toute couleur peut se poser ; null si aucune possible</returns>
        public ColorCh? OkColorFor(Coordinate coordinate, List<Square> squares, out int adjacentChrominos)
        {
            HashSet<ColorCh> colors = new HashSet<ColorCh>();
            ColorCh? rightColor = coordinate.RightColor(squares);
            ColorCh? bottomColor = coordinate.BottomColor(squares);
            ColorCh? leftColor = coordinate.LeftColor(squares);
            ColorCh? topColor = coordinate.TopColor(squares);
            adjacentChrominos = 0;
            if (rightColor != null)
            {
                adjacentChrominos++;
                colors.Add((ColorCh)rightColor);
            }
            if (bottomColor != null)
            {
                adjacentChrominos++;
                colors.Add((ColorCh)bottomColor);
            }
            if (leftColor != null)
            {
                adjacentChrominos++;
                colors.Add((ColorCh)leftColor);
            }
            if (topColor != null)
            {
                adjacentChrominos++;
                colors.Add((ColorCh)topColor);
            }

            if (colors.Count == 0)
            {
                return ColorCh.Cameleon;
            }
            else if (colors.Count == 1)
            {
                ColorCh theColor = new ColorCh();
                foreach (ColorCh color in colors)
                {
                    theColor = color;
                }
                return theColor;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// liste (sans doublons) des coordonées libres ajdacentes aux squares occupés.
        /// les coordonées des derniers squares occupés sont remontées en premiers.
        /// </summary>
        /// <param name="squares">liste de squares</param>
        /// <returns></returns>
        public HashSet<Coordinate> FreeAroundSquares(List<Square> squares)
        {
            HashSet<Coordinate> coordinates = new HashSet<Coordinate>();
            foreach (Square square in squares)
            {
                Coordinate rightCoordinate = square.GetRight();
                Coordinate bottomCoordinate = square.GetBottom();
                Coordinate leftCoordinate = square.GetLeft();
                Coordinate topCoordinate = square.GetTop();
                if (IsFree(ref squares, rightCoordinate))
                    coordinates.Add(rightCoordinate);
                if (IsFree(ref squares, bottomCoordinate))
                    coordinates.Add(bottomCoordinate);
                if (IsFree(ref squares, leftCoordinate))
                    coordinates.Add(leftCoordinate);
                if (IsFree(ref squares, topCoordinate))
                    coordinates.Add(topCoordinate);
            }
            return coordinates;
        }

        

        /// <summary>
        /// rempli la main de tous les joueurs
        /// </summary>
        /// <param name="gamePlayer"></param>

        private void FillHandPlayers()
        {
            foreach (GamePlayer gamePlayer in GamePlayers)
            {
                FillHand(gamePlayer);
            }
        }

        /// <summary>
        /// rempli la main du joueur de chrominos
        /// </summary>
        /// <param name="gamePlayer"></param>
        private void FillHand(GamePlayer gamePlayer)
        {
            for (int i = 0; i < BeginGameChrominoInHand; i++)
            {
                GameChrominoDal.StackToHand(GameId, gamePlayer.PlayerId);
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
        /// change l'ordre des n chrominos de la main s'il y a n-1 cameleon
        /// afin de jouer les cameleon et finir avec un chromino normal
        /// </summary>
        /// <param name="hand">référence de la liste des chrominos de la main du joueur</param>
        private void SortHandToFinishedWithoutCameleon(ref List<ChrominoInHand> hand)
        {
            if (hand.Count > 1)
            {
                bool forcePlayCameleon = true;
                for (int i = 1; i < hand.Count; i++)
                {
                    if (!ChrominoDal.IsCameleon(hand[i].ChrominoId))
                    {
                        forcePlayCameleon = false;
                        break;
                    }
                }
                if (forcePlayCameleon)
                {
                    ChrominoInHand chrominoAt0 = hand[0];
                    hand.RemoveAt(0);
                    hand.Add(chrominoAt0);
                }
            }
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
                if (GameChrominoDal.InHand(GameId, currentPlayerId) == 1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// retourne la position où le joueur peut jouer parmis la liste passée en paramètre
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        /// <param name="positions">liste des positions candidates</param>
        /// <returns>Position retenue. null si aucune</returns>
        private Position PositionWherePlayerCanPlay(int playerId, List<Position> positions)
        {
            List<ChrominoInHand> hand = GameChrominoDal.Hand(GameId, playerId);
            ComputeChrominoToPlay(hand, false, positions, out _, out Position position);
            return position;
        }

        /// <summary>
        /// retourne les positions possibles où peuvent être jouer des chrominos
        /// </summary>
        /// <param name="coordinates">ensemble des coordonnées potentielles</param>
        /// <param name="squares">liste des squares occupés</param>
        /// <returns></returns>
        private List<Position> ComputePossiblesPositions(HashSet<Coordinate> coordinates, List<Square> squares)
        {
            List<Position> positions = new List<Position>();
            // prendre ceux avec un seul coté en commun avec un chromino
            // calculer orientation avec les couleurs imposées ou pas
            foreach (Coordinate coordinate in coordinates)
            {
                ColorCh? firstColor = OkColorFor(coordinate, squares, out int commonSidesFirstColor);
                if (firstColor != null)
                {
                    //cherche placement possible d'un square
                    foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
                    {
                        Coordinate secondCoordinate = coordinate.GetNextCoordinate(orientation);
                        Coordinate thirdCoordinate = secondCoordinate.GetNextCoordinate(orientation);
                        if (IsFree(ref squares, secondCoordinate) && IsFree(ref squares, thirdCoordinate))
                        {
                            //calcul si chromino posable et dans quelle position
                            ColorCh? secondColor = OkColorFor(secondCoordinate, squares, out int adjacentChrominoSecondColor);
                            ColorCh? thirdColor = OkColorFor(thirdCoordinate, squares, out int adjacentChrominosThirdColor);

                            if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
                            {
                                Position possibleSpace = new Position
                                {
                                    Coordinate = coordinate,
                                    Orientation = orientation,
                                    FirstColor = firstColor,
                                    SecondColor = secondColor,
                                    ThirdColor = thirdColor,
                                };
                                positions.Add(possibleSpace);
                            }
                        }
                    }
                }
            }
            return positions;
        }

        /// <summary>
        /// Indique si avec les chrominos passés en paramètre, il est possible de jouer un de ces chrominos et le retourne
        /// </summary>
        /// <param name="hand">liste des chrominos à tester</param>
        /// <param name="previouslyDraw">indique si la joeur vient de piocher avant de tester</param>
        /// <param name="positions">liste des positions où chercher à placer le chromino</param>
        /// <param name="indexInHand">index du chromino convenant</param>
        /// <param name="position">position si convenant, null sinon</param>
        /// <returns>null si aucun chromino ne couvient</returns>
        private ChrominoInGame ComputeChrominoToPlay(List<ChrominoInHand> hand, bool previouslyDraw, List<Position> positions, out int indexInHand, out Position position)
        {
            ChrominoInGame goodChromino = null;
            Coordinate firstCoordinate;
            ChrominoInHand goodChrominoInHand;
            indexInHand = -1;
            position = null;
            if (!previouslyDraw && hand.Count == 1 && ChrominoDal.IsCameleon(hand[0].ChrominoId)) // on ne peut pas poser un cameleon si c'est le dernier de la main
            {
                goodChromino = null;
            }
            else
            {
                SortHandToFinishedWithoutCameleon(ref hand);
                foreach (ChrominoInHand chrominoInHand in hand)
                {
                    indexInHand++;
                    foreach (Position currentPosition in positions)
                    {
                        Chromino chromino = ChrominoDal.Details(chrominoInHand.ChrominoId);
                        if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon))
                        {
                            goodChrominoInHand = chrominoInHand;
                            firstCoordinate = currentPosition.Coordinate;
                            goodChromino = new ChrominoInGame()
                            {
                                Orientation = currentPosition.Orientation,
                                ChrominoId = chromino.Id,
                                GameId = GameId,
                                XPosition = firstCoordinate.X,
                                YPosition = firstCoordinate.Y,
                            };
                            position = currentPosition;
                            break;
                        }
                        else if ((chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon))
                        {
                            goodChrominoInHand = chrominoInHand;
                            firstCoordinate = currentPosition.Coordinate.GetNextCoordinate(currentPosition.Orientation).GetNextCoordinate(currentPosition.Orientation);
                            goodChromino = new ChrominoInGame()
                            {
                                Orientation = currentPosition.Orientation switch
                                {
                                    Orientation.Horizontal => Orientation.HorizontalFlip,
                                    Orientation.HorizontalFlip => Orientation.Horizontal,
                                    Orientation.Vertical => Orientation.VerticalFlip,
                                    _ => Orientation.Vertical,
                                },
                                ChrominoId = chromino.Id,
                                GameId = GameId,
                                XPosition = firstCoordinate.X,
                                YPosition = firstCoordinate.Y,
                            };
                            position = currentPosition;
                            break;
                        }
                    }
                    if (goodChromino != null)
                        break;
                }
            }
            return goodChromino;
        }

        private List<Square> ComputeSquares(ChrominoInGame chrominoIG)
        {
            int gameId = chrominoIG.GameId;
            int xOrigin = chrominoIG.XPosition;
            int yOrigin = chrominoIG.YPosition;
            Chromino chromino = ChrominoDal.Details(chrominoIG.ChrominoId);
            List<Square> squares = new List<Square>() { new Square {GameId = gameId, X = xOrigin, Y = yOrigin, Color=chromino.FirstColor } };
            switch (chrominoIG.Orientation)
            {
                case Orientation.Horizontal:
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 1, Y = yOrigin, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 2, Y = yOrigin, Color = chromino.ThirdColor });
                    break;
                case Orientation.HorizontalFlip:
                    squares.Add(new Square { GameId = gameId, X = xOrigin - 1, Y = yOrigin, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin - 2, Y = yOrigin, Color = chromino.ThirdColor });
                    break;
                case Orientation.Vertical:
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin - 1, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin - 2, Color = chromino.ThirdColor });
                    break;
                case Orientation.VerticalFlip:
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 1, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 2, Color = chromino.ThirdColor });
                    break;
            }
            return squares;
        }

        /// <summary>
        /// indique si à la coordonnée donnée, l'emplacement est libre
        /// </summary>
        /// <param name="squares">ensemble de squares dans lequel porter la recherche</param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public bool IsFree(ref List<Square> squares, Coordinate coordinate)
        {
            Square result = (from s in squares
                             where s.X == coordinate.X && s.Y == coordinate.Y
                             select s).FirstOrDefault();

            return result == null ? true : false;
        }
    }
}