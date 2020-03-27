using Data.Enumeration;
using Data.Models;
using System.Collections.Generic;
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
    }
}
