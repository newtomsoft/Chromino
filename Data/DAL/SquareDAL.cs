using Data.Core;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public partial class SquareDal
    {
        private readonly Context Ctx;

        public SquareDal(Context context)
        {
            Ctx = context;
        }

        public void Add(List<Square> squares)
        {
            foreach (var square in squares)
            {
                Ctx.Squares.Add(square);
                Ctx.SaveChanges();
            }
        }

        /// <summary>
        /// return number of squares with same color around this coordinate
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="coordinate"></param>
        /// <param name="color"></param>
        /// <returns>-1 if a square around have a color and is not the same color. Else the number </returns>
        public int GetNumberSameColorsAround(int gameId, Coordinate coordinate, ColorCh color)
        {
            int sameColors = 0;
            foreach (var offset in Coordinate.OffsetsAround)
            {
                ColorCh? currentColor = GetColor(gameId, coordinate + offset);
                if (currentColor != null)
                {
                    if (color == currentColor || color == ColorCh.Cameleon || currentColor == ColorCh.Cameleon)
                        sameColors++;
                    else
                        return -1;
                }
            }
            return sameColors;
        }

        public ColorCh? GetColor(int gameId, Coordinate coordinate)
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

        public int Delete(IQueryable<int> gamesIdToDelete)
        {
            var result = from s in Ctx.Squares
                         where gamesIdToDelete.Contains(s.GameId)
                         select s;

            Ctx.Squares.RemoveRange(result);
            return Ctx.SaveChanges();
        }
    }
}
