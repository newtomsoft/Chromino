using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Data.Core
{
    public class GameCore
    {
        private const int MaxChrominoInHand = 8;
        private readonly GameChrominoDal ChrominoGameDAL;
        private readonly ChrominoDal ChrominoDAL;
        private readonly SquareDAL SquareDAL;
        private readonly PlayerDal PlayerDAL;
        private readonly GamePlayerDal GamePlayerDAL;
        private readonly GameDal GameDAL;
        private int GameId { get; set; }
        public List<Square> Squares { get; set; }
        public List<Player> Players { get; set; }
        public List<GamePlayer> GamePlayers { get; set; }

        public GameCore(DefaultContext ctx, int gameId, List<Player> listPlayers)
        {
            GameDAL = new GameDal(ctx);
            ChrominoGameDAL = new GameChrominoDal(ctx);
            ChrominoDAL = new ChrominoDal(ctx);
            SquareDAL = new SquareDAL(ctx);
            PlayerDAL = new PlayerDal(ctx);
            GamePlayerDAL = new GamePlayerDal(ctx);
            GameId = gameId;
            Squares = SquareDAL.List(GameId);
            Players = listPlayers;
            InitGamePlayers();
        }

        private void InitGamePlayers()
        {
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
            {
                GamePlayers.Add(GamePlayerDAL.GamePlayer(GameId, player.Id));
            }
        }

        public void BeginGame()
        {
            GameDAL.SetStatus(GameId, GameStatus.InProgress);
            GameChromino firstChromino = ChrominoGameDAL.RandomFirstChromino(GameId);
            SquareDAL.PutChromino(firstChromino, new Coordinate(0, 0).GetPreviousCoordinate((Orientation)firstChromino.Orientation), true);
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
                        if (SquareDAL.IsFree(GameId, secondCoordinate) && SquareDAL.IsFree(GameId, thirdCoordinate))
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

            // cherche un chromino dans la main du joueur correspondant à un possibleSpace
            possiblesPositions = possiblesPositions.OrderBy(a => Guid.NewGuid()).ToList();
            List<GameChromino> hand = ChrominoGameDAL.PlayerListByPriority(GameId, Players[0].Id);
            GameChromino goodChrominoGame = null;
            Coordinate firstCoordinate = null;

            foreach (GameChromino chrominoGame in hand)
            {
                foreach (PossiblesPositions possibleSpace in possiblesPositions)
                {
                    Chromino chromino = ChrominoDAL.Details(chrominoGame.ChrominoId);

                    if ((chromino.FirstColor == possibleSpace.FirstColor || possibleSpace.FirstColor == Color.Cameleon) && (chromino.SecondColor == possibleSpace.SecondColor || chromino.SecondColor == Color.Cameleon || possibleSpace.SecondColor == Color.Cameleon) && (chromino.ThirdColor == possibleSpace.ThirdColor || possibleSpace.ThirdColor == Color.Cameleon))
                    {
                        chrominoGame.Orientation = possibleSpace.Orientation;
                        firstCoordinate = possibleSpace.Coordinate;
                        goodChrominoGame = chrominoGame;
                        break;
                    }
                    else if ((chromino.FirstColor == possibleSpace.ThirdColor || possibleSpace.ThirdColor == Color.Cameleon) && (chromino.SecondColor == possibleSpace.SecondColor || chromino.SecondColor == Color.Cameleon || possibleSpace.SecondColor == Color.Cameleon) && (chromino.ThirdColor == possibleSpace.FirstColor || possibleSpace.FirstColor == Color.Cameleon))
                    {
                        firstCoordinate = possibleSpace.Coordinate.GetNextCoordinate(possibleSpace.Orientation).GetNextCoordinate(possibleSpace.Orientation);
                        chrominoGame.Orientation = possibleSpace.Orientation switch
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
                GameChromino chrominoGame = ChrominoGameDAL.ChrominoFromStackToHandPlayer(GameId, GamePlayers[0].PlayerId);
                if (chrominoGame == null)
                    GameDAL.SetStatus(GameId, GameStatus.Finished);
                return false;
            }
            else
            {
                bool put = SquareDAL.PutChromino(goodChrominoGame, firstCoordinate);
                if (!put)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    ChrominoGameDAL.ChrominoFromStackToHandPlayer(GameId, GamePlayers[0].PlayerId);
                }
                return true;
            }
        }

        /// <summary>
        /// retourne la couleur possible à cette endroit
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
            List<Square> grids = SquareDAL.List(GameId);
            HashSet<Coordinate> coordinates = new HashSet<Coordinate>();
            foreach (var grid in grids)
            {
                Coordinate rightCoordinate = grid.GetRight();
                Coordinate bottomCoordinate = grid.GetBottom();
                Coordinate leftCoordinate = grid.GetLeft();
                Coordinate topCoordinate = grid.GetTop();
                if (SquareDAL.IsFree(GameId, rightCoordinate))
                    coordinates.Add(rightCoordinate);
                if (SquareDAL.IsFree(GameId, bottomCoordinate))
                    coordinates.Add(bottomCoordinate);
                if (SquareDAL.IsFree(GameId, leftCoordinate))
                    coordinates.Add(leftCoordinate);
                if (SquareDAL.IsFree(GameId, topCoordinate))
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
                ChrominoGameDAL.ChrominoFromStackToHandPlayer(GameId, gamePlayer.PlayerId);
            }
        }
    }
}
