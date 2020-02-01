using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data.Core;
using Data.Enumeration;

namespace Data.DAL
{
    public class SquareDal
    {
        private readonly DefaultContext Ctx;

        public SquareDal(DefaultContext context)
        {
            Ctx = context;
        }

        public bool PutChromino(GameChromino gameChromino, bool firstChromino = false)
        {
            ChrominoDal chrominoDal = new ChrominoDal(Ctx);
            Chromino chromino = chrominoDal.Details(gameChromino.ChrominoId);

            ComputeOffset((Orientation)gameChromino.Orientation, out int offsetX, out int offsetY);
            Coordinate firstCoordinate = new Coordinate((int)gameChromino.XPosition, (int)gameChromino.YPosition);
            Coordinate secondCoordinate = new Coordinate(firstCoordinate.X + offsetX, firstCoordinate.Y + offsetY);
            Coordinate thirdCoordinate = new Coordinate(firstCoordinate.X + 2 * offsetX, firstCoordinate.Y + 2 * offsetY);

            int gameId = gameChromino.GameId;

            if (!firstChromino && (!IsFree(gameId, firstCoordinate) || !IsFree(gameId, secondCoordinate) || !IsFree(gameId, thirdCoordinate)))
            {
                return false;
            }
            else
            {
                int n1, n2, n3;
                if (firstChromino)
                {
                    n1 = 2; n2 = 2; n3 = 2;
                }
                else
                {
                    n1 = 0; n2 = 0; n3 = 0;
                }
                // todo not the best method for validate first chromino position...

                if (!firstChromino && ((n1 = GetNumberSameColorsAround(gameId, firstCoordinate, chromino.FirstColor)) == -1 || (n2 = GetNumberSameColorsAround(gameId, secondCoordinate, chromino.SecondColor)) == -1 || (n3 = GetNumberSameColorsAround(gameId, thirdCoordinate, chromino.ThirdColor)) == -1))
                {
                    return false;
                }
                else if (!firstChromino && (n1 + n2 + n3 < 2))
                {
                    return false;
                }
                else
                {
                    //the position is valide :)
                    OpenEdge firstEdge;
                    OpenEdge secondEdge;
                    OpenEdge thirdEdge;
                    switch (gameChromino.Orientation)
                    {
                        case Orientation.Horizontal:
                            firstEdge = OpenEdge.Right;
                            secondEdge = OpenEdge.RightLeft;
                            thirdEdge = OpenEdge.Left;
                            break;
                        case Orientation.HorizontalFlip:
                            firstEdge = OpenEdge.Left;
                            secondEdge = OpenEdge.RightLeft;
                            thirdEdge = OpenEdge.Right;
                            break;
                        case Orientation.Vertical:
                            firstEdge = OpenEdge.Top;
                            secondEdge = OpenEdge.BottomTop;
                            thirdEdge = OpenEdge.Bottom;
                            break;
                        case Orientation.VerticalFlip:
                        default:
                            firstEdge = OpenEdge.Bottom;
                            secondEdge = OpenEdge.BottomTop;
                            thirdEdge = OpenEdge.Top;
                            break;
                    }
                    Ctx.Squares.Add(new Square { GameId = gameId, X = firstCoordinate.X, Y = firstCoordinate.Y, Color = chromino.FirstColor, Edge = firstEdge });
                    Ctx.Squares.Add(new Square { GameId = gameId, X = secondCoordinate.X, Y = secondCoordinate.Y, Color = chromino.SecondColor, Edge = secondEdge });
                    Ctx.Squares.Add(new Square { GameId = gameId, X = thirdCoordinate.X, Y = thirdCoordinate.Y, Color = chromino.ThirdColor, Edge = thirdEdge });
                    gameChromino.State = ChrominoStatus.InGame;
                    Ctx.SaveChanges();
                    return true;
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

        public List<Square> List(int gameId)
        {
            var result = (from s in Ctx.Squares
                          where s.GameId == gameId
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
