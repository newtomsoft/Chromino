using Data.Enumeration;

namespace Data.Core
{
    public class Position
    {
        public Coordinate Coordinate { get; set; }
        public Orientation Orientation { get; set; }
        public ColorCh? FirstColor { get; set; }
        public ColorCh? SecondColor { get; set; }
        public ColorCh? ThirdColor { get; set; }
    }
}
