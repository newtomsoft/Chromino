using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Core
{
    public class Coordinate
    {
        public static readonly List<Coordinate> OffsetsAround = new List<Coordinate> { new Coordinate(0, 1), new Coordinate(0, -1), new Coordinate(1, 0), new Coordinate(-1, 0) };
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// New coordinate for offset use
        /// </summary>
        /// <param name="orientation"></param>
        public Coordinate(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Horizontal:
                    X = 1;
                    break;
                case Orientation.HorizontalFlip:
                    X = -1;
                    break;
                case Orientation.Vertical:
                    Y = -1;
                    break;
                case Orientation.VerticalFlip:
                    Y = 1;
                    break;
            }
        }

        public static Coordinate operator *(int a, Coordinate c) => new Coordinate(a * c.X, a * c.Y);
        public static Coordinate operator +(Coordinate c1, Coordinate c2) => new Coordinate(c1.X + c2.X, c1.Y + c2.Y);
        public static Coordinate operator -(Coordinate c, Coordinate offset) => new Coordinate(c.X - offset.X, c.Y - offset.Y);

        public Coordinate GetRightCoordinate()
        {
            return new Coordinate(X + 1, Y);
        }

        public Coordinate GetBottomCoordinate()
        {
            return new Coordinate(X, Y + 1);
        }

        public Coordinate GetLeftCoordinate()
        {
            return new Coordinate(X - 1, Y);
        }

        public Coordinate GetTopCoordinate()
        {
            return new Coordinate(X, Y - 1);
        }

        private ColorCh? GetColor(List<Square> grid, Coordinate coordinate)
        {
            Square square = (from g in grid
                             where g.X == coordinate.X && g.Y == coordinate.Y
                             select g).FirstOrDefault();

            if (square == null)
                return null;
            else
                return square.Color;
        }

        public ColorCh? RightColor(List<Square> grid)
        {
            Coordinate coordinate = GetRightCoordinate();
            return GetColor(grid, coordinate);
        }

        public ColorCh? BottomColor(List<Square> grid)
        {
            Coordinate coordinate = GetBottomCoordinate();
            return GetColor(grid, coordinate);
        }

        public ColorCh? LeftColor(List<Square> grid)
        {
            Coordinate coordinate = GetLeftCoordinate();
            return GetColor(grid, coordinate);
        }

        public ColorCh? TopColor(List<Square> grid)
        {
            Coordinate coordinate = GetTopCoordinate();
            return GetColor(grid, coordinate);
        }


        /// <summary>
        /// indique si l'emplacement est libre parmis les squares renseignés
        /// </summary>
        /// <param name="squares">ensemble de squares dans lequel porter la recherche</param>
        /// <returns></returns>
        public bool IsFree(ref List<Square> squares)
        {
            Square result = (from s in squares
                             where s.X == X && s.Y == Y
                             select s).FirstOrDefault();

            return result == null ? true : false;
        }

        /// <summary>
        /// retourne la couleur possible à cet endroit
        /// </summary>
        /// <param name="adjacentChrominos">nombre de coté adjacents à un chromino </param>
        /// <returns>Cameleon si toute les couleurs peuvent se poser ; null si aucune ne peut</returns>
        public ColorCh? OkColorFor(List<Square> squares, out int adjacentChrominos)
        {
            HashSet<ColorCh> colors = new HashSet<ColorCh>();
            adjacentChrominos = 0;
            foreach (var offset in OffsetsAround)
            {
                ColorCh? color = GetColor(squares, this + offset);
                if (color != null)
                {
                    adjacentChrominos++;
                    colors.Add((ColorCh)color);
                }
            }
            if (colors.Count == 0)
                return ColorCh.Cameleon;
            else if (colors.Count == 1)
                return colors.First();
            else
                return null;
        }
    }
}
