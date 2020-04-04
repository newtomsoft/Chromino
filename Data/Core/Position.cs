using Data.Enumeration;

namespace Data.Core
{
    public class Position
    {
        public Coordinate Coordinate { get; set; }
        public Orientation Orientation { get; set; }
        public bool Reversed { get; set; }
        public ColorCh? FirstColor { get; set; }
        public ColorCh? SecondColor { get; set; }
        public ColorCh? ThirdColor { get; set; }

        public Position() { }
        public Position(Position p)
        {
            Coordinate = p.Coordinate;
            Orientation = p.Orientation;
            Reversed = p.Reversed;
            FirstColor = p.FirstColor;
            SecondColor = p.SecondColor;
            ThirdColor = p.ThirdColor;
        }
    }
}
