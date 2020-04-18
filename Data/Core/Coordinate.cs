using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Core
{
    public class Coordinate
    {
        public static readonly Coordinate StepX = new Coordinate(1, 0);
        public static readonly Coordinate StepY = new Coordinate(0, 1);
        public static readonly List<Coordinate> OffsetsAround = new List<Coordinate> { StepX, -StepX, StepY, -StepY };

        public int X { get; set; }
        public int Y { get; set; }

        public static Coordinate operator *(int a, Coordinate c) => new Coordinate(a * c.X, a * c.Y);
        public static Coordinate operator *(Coordinate c, int a) => new Coordinate(a * c.X, a * c.Y);
        public static Coordinate operator +(Coordinate c1, Coordinate c2) => new Coordinate(c1.X + c2.X, c1.Y + c2.Y);
        public static Coordinate operator -(Coordinate c, Coordinate offset) => new Coordinate(c.X - offset.X, c.Y - offset.Y);
        public static Coordinate operator -(Coordinate c) => new Coordinate(-c.X, -c.Y);
        public static bool operator ==(Coordinate c1, Coordinate c2) => c1.X == c2.X && c2.Y == c2.Y;
        public static bool operator !=(Coordinate c1, Coordinate c2) => c1.X != c2.X || c2.Y != c2.Y;

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
                return false;
            else
            {
                Coordinate c = (Coordinate)obj;
                return (X == c.X) && (Y == c.Y);
            }
        }

        public override int GetHashCode() => (X * 1000 + Y).GetHashCode();


        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coordinate Abs()
        {
            return new Coordinate(Math.Abs(X), Math.Abs(Y));
        }

        /// <summary>
        /// New coordinate for offset use
        /// </summary>
        /// <param name="orientation"></param>
        public Coordinate(Orientation orientation, bool flip = false)
        {
            if (orientation == Orientation.Horizontal)
                X = flip ? -1 : 1;
            else if (orientation == Orientation.Vertical)
                Y = flip ? -1 : 1;
            else
                throw new Exception("Exception Orientation non définit");
        }


        public Orientation GetOrientation()
        {
            if (X == 0 && Y == 0)
                throw new Exception("Offset nul");
            return X == 0 ? Orientation.Vertical : Orientation.Horizontal;
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

        /// <summary>
        /// indique si l'emplacement est libre parmis les squares renseignés
        /// </summary>
        /// <param name="squares">ensemble de squares dans lequel porter la recherche</param>
        /// <returns></returns>
        public bool IsFree(List<Square> squares)
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

        public bool IsFreeForChromino(Coordinate offset, List<Square> squares)
        {
            for (int i = 1; i < 2; i++)
            {
                if (!(this + i * offset).IsFree(squares))
                    return false;
            }
            return true;
        }
    }
}
