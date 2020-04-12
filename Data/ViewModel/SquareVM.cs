using Data.Enumeration;

namespace Data.ViewModel
{
    public class SquareVM
    {
        public ColorCh Color { get; set; }
        public bool OpenRight { get; set; }
        public bool OpenBottom { get; set; }
        public bool OpenLeft { get; set; }
        public bool OpenTop { get; set; }

        public SquareVM(ColorCh color, bool openRight = false, bool openBottom = false, bool openLeft = false, bool openTop = false)
        {
            Color = color;
            OpenRight = openRight;
            OpenBottom = openBottom;
            OpenLeft = openLeft;
            OpenTop = openTop;
        }
    }
}
