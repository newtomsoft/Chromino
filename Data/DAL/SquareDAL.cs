using Data.Core;
using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        public void Add(List<Square> squares)
        {
            Ctx.Squares.AddRange(squares);
            Ctx.SaveChanges();
        }

        /// <summary>
        /// return number of squares with same color around this coordinate
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="coordinate"></param>
        /// <param name="color"></param>
        /// <returns>-1 if a square around have a color and is not the same color. Else the number </returns>
        public int GetNumberSameColorsAround(int gameId, Coordinate coordinate, Color color)
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

        public void ComputeOffset(Orientation orientation, out int offsetX, out int offsetY)
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
