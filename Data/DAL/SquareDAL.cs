using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data.Core;
using Data.Enumeration;

namespace Data.DAL
{
    public class SquareDAL
    {
        private readonly DefaultContext Ctx;

        public SquareDAL(DefaultContext context)
        {
            Ctx = context;
        }

        public bool PutChromino(GameChromino chromino_Game, Coordinate firstCoordinate, bool firstChromino = false)
        {
            ChrominoDal chrominoDAL = new ChrominoDal(Ctx);
            Chromino chromino = chrominoDAL.Details(chromino_Game.ChrominoId);

            ComputeOffset((Orientation)chromino_Game.Orientation, out int offsetX, out int offsetY);

            Coordinate secondCoordinate = new Coordinate(firstCoordinate.X + offsetX, firstCoordinate.Y + offsetY);
            Coordinate thirdCoordinate = new Coordinate(firstCoordinate.X + 2 * offsetX, firstCoordinate.Y + 2 * offsetY);

            int gameId = chromino_Game.GameId;

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
                //not the best method for validate first chromino position...

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
                    switch (chromino_Game.Orientation)
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

                    Ctx.Grids.Add(new Square { GameId = gameId, X = firstCoordinate.X, Y = firstCoordinate.Y, Color = chromino.FirstColor, Edge = firstEdge });
                    Ctx.Grids.Add(new Square { GameId = gameId, X = secondCoordinate.X, Y = secondCoordinate.Y, Color = chromino.SecondColor, Edge = secondEdge });
                    Ctx.Grids.Add(new Square { GameId = gameId, X = thirdCoordinate.X, Y = thirdCoordinate.Y, Color = chromino.ThirdColor, Edge = thirdEdge });

                    chromino_Game.XPosition = firstCoordinate.X;
                    chromino_Game.YPosition = firstCoordinate.Y;
                    chromino_Game.State = ChrominoStatus.InGame;

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
            var result = (from grids in Ctx.Grids
                          where grids.GameId == gameId && grids.X == coordinate.X && grids.Y == coordinate.Y
                          select grids).FirstOrDefault();

            if (result == null)
                return true;
            else
                return false;
        }

        public Color? GetColor(int gameId, Coordinate coordinate)
        {
            var result = (from grids in Ctx.Grids
                          where grids.GameId == gameId && grids.X == coordinate.X && grids.Y == coordinate.Y
                          select grids).FirstOrDefault();

            if (result != null)
                return result.Color;
            else
                return null;
        }

        public List<Square> List(int gameId)
        {
            var result = (from grids in Ctx.Grids
                          where grids.GameId == gameId
                          select grids).ToList();

            return result;
        }

        public int GetXMin(int gameId)
        {
            var result = (from grids in Ctx.Grids
                          where grids.GameId == gameId
                          select grids.X).Min();

            return result;

        }

        public int GetYMin(int gameId)
        {
            var result = (from grids in Ctx.Grids
                          where grids.GameId == gameId
                          select grids.Y).Min();

            return result;

        }

        public int GetXMax(int gameId)
        {
            var result = (from grids in Ctx.Grids
                          where grids.GameId == gameId
                          select grids.X).Max();

            return result;

        }

        public int GetYMax(int gameId)
        {
            var result = (from grids in Ctx.Grids
                          where grids.GameId == gameId
                          select grids.Y).Max();

            return result;

        }

        /// <summary>
        /// For test only
        /// </summary>
        /// <param name="chromino_Game"></param>
        public void RandomPutChromino(GameChromino chromino_Game)
        {
            Random random = new Random();
            bool put = false;
            while (!put)
            {
                Coordinate coordinate = new Coordinate(random.Next(-10, 10), random.Next(-10, 10));
                put = PutChromino(chromino_Game, coordinate);
            }
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
