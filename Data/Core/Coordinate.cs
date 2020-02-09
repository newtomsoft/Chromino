using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Data.Core
{
    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coordinate GetNextCoordinate(Orientation orientation)
        {
            return orientation switch
            {
                Orientation.Horizontal => GetRightCoordinate(),
                Orientation.HorizontalFlip => GetLeftCoordinate(),
                Orientation.Vertical => GetTopCoordinate(),
                _ => GetBottomCoordinate(),
            };
        }

        public Coordinate GetPreviousCoordinate(Orientation orientation)
        {
            return orientation switch
            {
                Orientation.Horizontal => GetLeftCoordinate(),
                Orientation.HorizontalFlip => GetRightCoordinate(),
                Orientation.Vertical => GetBottomCoordinate(),
                _ => GetTopCoordinate(),
            };
        }

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

        public Color? GetRightColor(List<Square> grid)
        {
            Coordinate coordinate = GetRightCoordinate();
            Square square = (from g in grid
                             where g.X == coordinate.X && g.Y == coordinate.Y
                             select g).FirstOrDefault();
            
            if (square == null)
                return null;
            else
                return square.Color;
        }

        public Color? GetBottomColor(List<Square> grid)
        {
            Coordinate coordinate = GetBottomCoordinate();
            Square square = (from g in grid
                             where g.X == coordinate.X && g.Y == coordinate.Y
                             select g).FirstOrDefault();
            
            if (square == null)
                return null;
            else
                return square.Color;
        }

        public Color? GetLeftColor(List<Square> grid)
        {
            Coordinate coordinate = GetLeftCoordinate();
            Square square = (from g in grid
                             where g.X == coordinate.X && g.Y == coordinate.Y
                             select g).FirstOrDefault();

            if (square == null)
                return null;
            else
                return square.Color;
        }

        public Color? GetTopColor(List<Square> grid)
        {
            Coordinate coordinate = GetTopCoordinate();
            Square square = (from g in grid
                             where g.X == coordinate.X && g.Y == coordinate.Y
                             select g).FirstOrDefault();

            if (square == null)
                return null;
            else
                return square.Color;
        }

    }
}
