using Data.Enumeration;

namespace Data.Core
{
    public class PossiblesPositions
    {
        public Coordinate Coordinate { get; set; }
        public Orientation Orientation { get; set; }
        public Color? FirstColor { get; set; }
        public Color? SecondColor { get; set; }
        public Color? ThirdColor { get; set; }
    }
}
