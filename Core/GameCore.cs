using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Core
{
    public class GameCore
    {
        private const int MaxChrominoInHand = 8;
        private readonly GameChrominoDal ChrominoGameDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;
        private int GameId { get; set; }
        public List<Square> Squares { get; set; }
        public List<Player> Players { get; set; }
        public List<GamePlayer> GamePlayers { get; set; }

        public GameCore(DefaultContext ctx, int gameId, List<Player> listPlayers)
        {
            GameDal = new GameDal(ctx);
            ChrominoGameDal = new GameChrominoDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            GameId = gameId;
            Squares = SquareDal.List(GameId);
            Players = listPlayers;
            InitGamePlayers();
        }

        private void InitGamePlayers()
        {
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
            {
                GamePlayers.Add(GamePlayerDal.GamePlayer(GameId, player.Id));
            }
        }

        public void BeginGame()
        {
            GameDal.SetStatus(GameId, GameStatus.InProgress);
            GameChromino firstChromino = ChrominoGameDal.RandomFirstChromino(GameId);
            SquareDal.PutChromino(firstChromino, new Coordinate(0, 0).GetPreviousCoordinate((Orientation)firstChromino.Orientation), true);
            FillHandPlayers();
            ChangePlayerTurn();
        }

        public void ChangePlayerTurn()
        {
            if (GamePlayers.Count == 1)
            {
                GamePlayers[0].PlayerTurn = true;
            }
            else
            {
                GamePlayer gamePlayer = (from gp in GamePlayers
                                          where gp.PlayerTurn == true
                                          select gp).FirstOrDefault();

                if (gamePlayer == null)
                {
                    gamePlayer = (from gp in GamePlayers
                                  orderby Guid.NewGuid()
                                  select gp).FirstOrDefault();
                }

                bool selectNext = false;
                bool selected = false;
                foreach (GamePlayer currentGamePlayer in GamePlayers)
                {
                    if (selectNext)
                    {
                        currentGamePlayer.PlayerTurn = true;
                        selected = true;
                        break;
                    }
                    if (gamePlayer.Id == currentGamePlayer.Id)
                        selectNext = true;
                }
                if (!selected)
                    GamePlayers[0].PlayerTurn = true;

                gamePlayer.PlayerTurn = false;
            }
        }

        /// <summary>
        /// placement d'un autre chrominos de la main du bot dans le jeu
        /// </summary>
        /// <param name="gameId"></param>
        public bool ContinueRandomGame()
        {
            HashSet<Coordinate> coordinates = FreeAroundChrominos();

            // prendre ceux avec un seul coté en commun avec un chrominos
            // calculer orientation avec les couleurs imposées ou pas

            List<PossiblesPositions> possiblesPositions = new List<PossiblesPositions>();
            foreach (Coordinate coordinate in coordinates)
            {
                Color? firstColor = OkColorFor(coordinate, out int commonSidesFirstColor);
                if (firstColor != null)
                {
                    //cherche placement possible d'un square
                    foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
                    {
                        Coordinate secondCoordinate = coordinate.GetNextCoordinate(orientation);
                        Coordinate thirdCoordinate = secondCoordinate.GetNextCoordinate(orientation);
                        if (SquareDal.IsFree(GameId, secondCoordinate) && SquareDal.IsFree(GameId, thirdCoordinate))
                        {
                            //calcul si chromino posable et dans quelle position
                            Color? secondColor = OkColorFor(secondCoordinate, out int adjacentChrominoSecondColor);
                            Color? thirdColor = OkColorFor(thirdCoordinate, out int adjacentChrominosThirdColor);

                            if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
                            {
                                PossiblesPositions possibleSpace = new PossiblesPositions
                                {
                                    Coordinate = coordinate,
                                    Orientation = orientation,
                                    FirstColor = firstColor,
                                    SecondColor = secondColor,
                                    ThirdColor = thirdColor,
                                };
                                possiblesPositions.Add(possibleSpace);
                            }
                        }
                    }
                }
            }

            // cherche un chromino dans la main du joueur correspondant à un possiblesPosition
            possiblesPositions = possiblesPositions.OrderBy(a => Guid.NewGuid()).ToList();
            List<GameChromino> hand = ChrominoGameDal.PlayerListByPriority(GameId, Players[0].Id);
            GameChromino goodChrominoGame = null;
            Coordinate firstCoordinate = null;

            foreach (GameChromino chrominoGame in hand)
            {
                foreach (PossiblesPositions possiblePosition in possiblesPositions)
                {
                    Chromino chromino = ChrominoDal.Details(chrominoGame.ChrominoId);

                    if ((chromino.FirstColor == possiblePosition.FirstColor || possiblePosition.FirstColor == Color.Cameleon) && (chromino.SecondColor == possiblePosition.SecondColor || chromino.SecondColor == Color.Cameleon || possiblePosition.SecondColor == Color.Cameleon) && (chromino.ThirdColor == possiblePosition.ThirdColor || possiblePosition.ThirdColor == Color.Cameleon))
                    {
                        chrominoGame.Orientation = possiblePosition.Orientation;
                        firstCoordinate = possiblePosition.Coordinate;
                        goodChrominoGame = chrominoGame;
                        break;
                    }
                    else if ((chromino.FirstColor == possiblePosition.ThirdColor || possiblePosition.ThirdColor == Color.Cameleon) && (chromino.SecondColor == possiblePosition.SecondColor || chromino.SecondColor == Color.Cameleon || possiblePosition.SecondColor == Color.Cameleon) && (chromino.ThirdColor == possiblePosition.FirstColor || possiblePosition.FirstColor == Color.Cameleon))
                    {
                        firstCoordinate = possiblePosition.Coordinate.GetNextCoordinate(possiblePosition.Orientation).GetNextCoordinate(possiblePosition.Orientation);
                        chrominoGame.Orientation = possiblePosition.Orientation switch
                        {
                            Orientation.Horizontal => Orientation.HorizontalFlip,
                            Orientation.HorizontalFlip => Orientation.Horizontal,
                            Orientation.Vertical => Orientation.VerticalFlip,
                            _ => Orientation.Vertical,
                        };
                        goodChrominoGame = chrominoGame;
                        break;
                    }
                }
                if (goodChrominoGame != null)
                    break;
            }

            if (goodChrominoGame == null)
            {
                GameChromino chrominoGame = ChrominoGameDal.ChrominoFromStackToHandPlayer(GameId, GamePlayers[0].PlayerId);
                if (chrominoGame == null)
                    GameDal.SetStatus(GameId, GameStatus.Finished);
                return false;
            }
            else
            {
                bool put = SquareDal.PutChromino(goodChrominoGame, firstCoordinate);
                if (!put)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    ChrominoGameDal.ChrominoFromStackToHandPlayer(GameId, GamePlayers[0].PlayerId);
                }
                return true;
            }
        }

        /// <summary>
        /// retourne la couleur possible à cet endroit
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="adjacentChrominos"></param>
        /// <returns>Cameleon si toute couleur peut se poser ; null si aucune possible</returns>
        public Color? OkColorFor(Coordinate coordinate, out int adjacentChrominos)
        {
            HashSet<Color> colors = new HashSet<Color>();
            Color? rightColor = coordinate.GetRightColor(Squares);
            Color? bottomColor = coordinate.GetBottomColor(Squares);
            Color? leftColor = coordinate.GetLeftColor(Squares);
            Color? topColor = coordinate.GetTopColor(Squares);
            adjacentChrominos = 0;
            if (rightColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)rightColor);
            }
            if (bottomColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)bottomColor);
            }
            if (leftColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)leftColor);
            }
            if (topColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)topColor);
            }

            if (colors.Count == 0)
            {
                return Color.Cameleon; 
            }
            else if (colors.Count == 1)
            {
                Color theColor = new Color();
                foreach (Color color in colors)
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

        public HashSet<Coordinate> FreeAroundChrominos()
        {
            List<Square> grids = SquareDal.List(GameId);
            HashSet<Coordinate> coordinates = new HashSet<Coordinate>();
            foreach (var grid in grids)
            {
                Coordinate rightCoordinate = grid.GetRight();
                Coordinate bottomCoordinate = grid.GetBottom();
                Coordinate leftCoordinate = grid.GetLeft();
                Coordinate topCoordinate = grid.GetTop();
                if (SquareDal.IsFree(GameId, rightCoordinate))
                    coordinates.Add(rightCoordinate);
                if (SquareDal.IsFree(GameId, bottomCoordinate))
                    coordinates.Add(bottomCoordinate);
                if (SquareDal.IsFree(GameId, leftCoordinate))
                    coordinates.Add(leftCoordinate);
                if (SquareDal.IsFree(GameId, topCoordinate))
                    coordinates.Add(topCoordinate);
            }
            return coordinates;
        }


        private void FillHandPlayers()
        {
            foreach (GamePlayer gamePlayer in GamePlayers)
            {
                FillHand(gamePlayer);
            }
        }

        private void FillHand(GamePlayer gamePlayer)
        {
            for (int i = 0; i < MaxChrominoInHand; i++)
            {
                ChrominoGameDal.ChrominoFromStackToHandPlayer(GameId, gamePlayer.PlayerId);
            }
        }
    }
}
