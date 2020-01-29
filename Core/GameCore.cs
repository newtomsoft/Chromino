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
        private const int BotId = 1;
        private readonly DefaultContext Ctx;
        private readonly GameChrominoDal GameChrominoDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;
        private int GameId { get; set; }
        public List<Square> Squares { get; set; }
        public List<Player> Players { get; set; }
        public List<GamePlayer> GamePlayers { get; set; }

        public GameCore(DefaultContext ctx, int gameId, List<Player> listPlayers = null)
        {
            Ctx = ctx;
            GameDal = new GameDal(ctx);
            GameChrominoDal = new GameChrominoDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);

            if (listPlayers == null)
            {
                Players = GamePlayerDal.Players(gameId);
            }
            else
            {
                Players = listPlayers;
            }



            GameId = gameId;
            Squares = SquareDal.List(GameId);
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
            {
                GamePlayers.Add(GamePlayerDal.GamePlayer(GameId, player.Id));
            }




        }

        public void BeginGame()
        {
            GameDal.SetStatus(GameId, GameStatus.InProgress);
            GameChromino firstChromino = GameChrominoDal.RandomFirstChromino(GameId);
            SquareDal.PutChromino(firstChromino, true);
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
            Ctx.SaveChanges();
        }

        /// <summary>
        /// placement d'un autre chrominos de la main du bot dans le jeu
        /// </summary>
        /// <param name="gameId"></param>
        public bool PlayBot()
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

            // cherche un chromino dans la main du bot correspondant à un possiblesPosition
            possiblesPositions = possiblesPositions.OrderBy(a => Guid.NewGuid()).ToList();
            List<GameChromino> hand = GameChrominoDal.ChrominosByPriority(GameId, BotId);
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

                GameChromino gameChromino = GameChrominoDal.ChrominoFromStackToHandPlayer(GameId, BotId);
                //todo pioche systématique uniquement si bot seul à jouer
                if (gameChromino == null)
                    GameDal.SetStatus(GameId, GameStatus.Finished);
                // todo finish uniquement si bot seul à jouer
                return false;
            }
            else
            {
                goodChrominoGame.XPosition = firstCoordinate.X;
                goodChrominoGame.YPosition = firstCoordinate.Y;
                bool put = SquareDal.PutChromino(goodChrominoGame);
                if (put)
                {
                    return true;
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    GameChrominoDal.ChrominoFromStackToHandPlayer(GameId, BotId);
                    return false;
                }
            }
        }

        public bool Play(GameChromino gameChromino)
        {
            Coordinate coordinate;
            switch (gameChromino.Orientation)
            {
                case Orientation.HorizontalFlip:
                    coordinate = new Coordinate((int)gameChromino.XPosition + 2, (int)gameChromino.YPosition);
                    break;
                case Orientation.Vertical:
                    coordinate = new Coordinate((int)gameChromino.XPosition, (int)gameChromino.YPosition + 2);
                    break;
                case Orientation.Horizontal:
                case Orientation.VerticalFlip:
                default:
                    coordinate = new Coordinate((int)gameChromino.XPosition, (int)gameChromino.YPosition);
                    break;
            }
            gameChromino.XPosition = coordinate.X;
            gameChromino.YPosition = coordinate.Y;
            bool put = SquareDal.PutChromino(gameChromino);
            if (put)
            {
                gameChromino.State = ChrominoStatus.InGame;
                GamePlayerDal.AddPoint((int)gameChromino.PlayerId, ChrominoDal.Details(gameChromino.ChrominoId).Points);
                //todo ? reste calculs à faire ?
            }
            return put;
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
                GameChrominoDal.ChrominoFromStackToHandPlayer(GameId, gamePlayer.PlayerId);
            }
        }
    }
}
