using Data.Core;
using Data.Enumeration;
using Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class SquareDal
    {
        private readonly Context Ctx;

        public SquareDal(Context context)
        {
            Ctx = context;
        }

        /// <summary>
        /// put a chrominoInGame in game (playerId = 0 : first chromino in center)
        /// </summary>
        /// <param name="chrominoInGame"></param>
        /// <param name="playerId">0 for first chromino</param>
        /// <returns></returns>
        public PlayReturn PlayChromino(ChrominoInGame chrominoInGame, int? playerId)
        {
            Chromino chromino = new ChrominoDal(Ctx).Details(chrominoInGame.ChrominoId);

            ComputeOffset(chrominoInGame.Orientation, out int offsetX, out int offsetY);
            Coordinate firstCoordinate = new Coordinate(chrominoInGame.XPosition, chrominoInGame.YPosition);
            Coordinate secondCoordinate = new Coordinate(firstCoordinate.X + offsetX, firstCoordinate.Y + offsetY);
            Coordinate thirdCoordinate = new Coordinate(firstCoordinate.X + 2 * offsetX, firstCoordinate.Y + 2 * offsetY);

            int gameId = chrominoInGame.GameId;

            if (playerId > 0 && (!IsFree(gameId, firstCoordinate) || !IsFree(gameId, secondCoordinate) || !IsFree(gameId, thirdCoordinate)))
            {
                return PlayReturn.NotFree;
            }
            else
            {
                int n1, n2, n3;
                if (playerId == 0) // le premier chromino posé par aucun joueur
                {
                    n1 = 2; n2 = 2; n3 = 2;
                }
                else
                {
                    n1 = 0; n2 = 0; n3 = 0;
                }
                // todo not the best method for validate first chromino position...

                if (playerId > 0 && ((n1 = GetNumberSameColorsAround(gameId, firstCoordinate, chromino.FirstColor)) == -1 || (n2 = GetNumberSameColorsAround(gameId, secondCoordinate, chromino.SecondColor)) == -1 || (n3 = GetNumberSameColorsAround(gameId, thirdCoordinate, chromino.ThirdColor)) == -1))
                {
                    return PlayReturn.DifferentColorsAround;
                }
                else if (playerId > 0 && (n1 + n2 + n3 < 2))
                {
                    return PlayReturn.NotTwoOrMoreSameColors;
                }
                else
                {
                    //the position is ok :)
                    bool firstSquareOpenRight = false, firstSquareOpenBottom = false, firstSquareOpenLeft = false, firstSquareOpenTop = false;
                    bool secondSquareOpenRight = false, secondSquareOpenBottom = false, secondSquareOpenLeft = false, secondSquareOpenTop = false;
                    bool thirdSquareOpenRight = false, thirdSquareOpenBottom = false, thirdSquareOpenLeft = false, thirdSquareOpenTop = false;
                    switch (chrominoInGame.Orientation)
                    {
                        case Orientation.Horizontal:
                            firstSquareOpenRight = true;
                            secondSquareOpenRight = true;
                            secondSquareOpenLeft = true;
                            thirdSquareOpenLeft = true;
                            break;
                        case Orientation.HorizontalFlip:
                            firstSquareOpenLeft = true;
                            secondSquareOpenRight = true;
                            secondSquareOpenLeft = true;
                            thirdSquareOpenRight = true;
                            break;
                        case Orientation.Vertical:
                            firstSquareOpenTop = true;
                            secondSquareOpenTop = true;
                            secondSquareOpenBottom = true;
                            thirdSquareOpenBottom = true;
                            break;
                        case Orientation.VerticalFlip:
                            firstSquareOpenBottom = true;
                            secondSquareOpenTop = true;
                            secondSquareOpenBottom = true;
                            thirdSquareOpenTop = true;
                            break;
                    }
                    Ctx.Squares.Add(new Square { GameId = gameId, X = firstCoordinate.X, Y = firstCoordinate.Y, Color = chromino.FirstColor, OpenRight = firstSquareOpenRight, OpenBottom = firstSquareOpenBottom, OpenLeft = firstSquareOpenLeft, OpenTop = firstSquareOpenTop });
                    Ctx.Squares.Add(new Square { GameId = gameId, X = secondCoordinate.X, Y = secondCoordinate.Y, Color = chromino.SecondColor, OpenRight = secondSquareOpenRight, OpenBottom = secondSquareOpenBottom, OpenLeft = secondSquareOpenLeft, OpenTop = secondSquareOpenTop });
                    Ctx.Squares.Add(new Square { GameId = gameId, X = thirdCoordinate.X, Y = thirdCoordinate.Y, Color = chromino.ThirdColor, OpenRight = thirdSquareOpenRight, OpenBottom = thirdSquareOpenBottom, OpenLeft = thirdSquareOpenLeft, OpenTop = thirdSquareOpenTop });
                    chrominoInGame.Move = (byte)(new GameChrominoDal(Ctx).MaxMove(gameId) + 1);
                    chrominoInGame.PlayerId = playerId;
                    Ctx.ChrominosInGame.Add(chrominoInGame);
                    new GameChrominoDal(Ctx).DeleteInHand(gameId, chromino.Id);
                    Ctx.SaveChanges();
                    return PlayReturn.Ok;
                }
            }
        }

        /// <summary>
        /// return number of squares with same color around this coordinate
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="coordinate"></param>
        /// <param name="color"></param>
        /// <returns>-1 if a square around have a color and is not the same color. Else the number </returns>
        private int GetNumberSameColorsAround(int gameId, Coordinate coordinate, Color color)
        {
            int sameColors = 0;
            for (int i = 0; i < 4; i++)
            {
                Color? colorToCompare = i switch
                {
                    0 => GetColor(gameId, coordinate.GetRightCoordinate()),
                    1 => GetColor(gameId, coordinate.GetBottomCoordinate()),
                    2 => GetColor(gameId, coordinate.GetLeftCoordinate()),
                    3 => GetColor(gameId, coordinate.GetTopCoordinate()),
                    _ => GetColor(gameId, coordinate.GetRightCoordinate()),
                };
                if (colorToCompare != null)
                {
                    if (color == colorToCompare || color == Color.Cameleon || colorToCompare == Color.Cameleon)
                        sameColors++;
                    else
                        return -1;
                }
            }
            return sameColors;
        }

        public bool IsFree(int gameId, Coordinate coordinate)
        {
            var result = (from s in Ctx.Squares
                          where s.GameId == gameId && s.X == coordinate.X && s.Y == coordinate.Y
                          select s).FirstOrDefault();

            if (result == null)
                return true;
            else
                return false;
        }

        public Color? GetColor(int gameId, Coordinate coordinate)
        {
            var result = (from s in Ctx.Squares
                          where s.GameId == gameId && s.X == coordinate.X && s.Y == coordinate.Y
                          select s).FirstOrDefault();

            if (result != null)
                return result.Color;
            else
                return null;
        }

        /// <summary>
        /// liste des squares du jeu classés par ceux occupés en dernier (par des chrominos)
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        public List<Square> List(int gameId)
        {
            var result = (from s in Ctx.Squares
                          where s.GameId == gameId
                          orderby s.Id descending
                          select s).ToList();

            return result;
        }

        private void ComputeOffset(Orientation orientation, out int offsetX, out int offsetY)
        {
            offsetX = 0;
            offsetY = 0;
            switch (orientation)
            {
                case Orientation.Horizontal:
                    offsetX = 1;
                    break;
                case Orientation.HorizontalFlip:
                    offsetX = -1;
                    break;
                case Orientation.Vertical:
                    offsetY = -1;
                    break;
                case Orientation.VerticalFlip:
                    offsetY = 1;
                    break;
                default:
                    offsetX = -1;
                    break;
            }
        }

    }
}
