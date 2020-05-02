using Data.BI;
using Data.ViewModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    public partial class Square
    {
        [NotMapped]
        public SquareVM SquareVM { get => new SquareVM(Color, OpenRight, OpenBottom, OpenLeft, OpenTop); }

        [NotMapped]
        public Coordinate Coordinate { get => new Coordinate(X, Y); }

        public Coordinate GetRight()
        {
            return new Coordinate(X + 1, Y);
        }

        public Coordinate GetBottom()
        {
            return new Coordinate(X, Y + 1);
        }

        public Coordinate GetLeft()
        {
            return new Coordinate(X - 1, Y);
        }

        public Coordinate GetTop()
        {
            return new Coordinate(X, Y - 1);
        }
    }
}
