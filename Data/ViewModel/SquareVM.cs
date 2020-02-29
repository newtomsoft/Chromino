using Data.Enumeration;

namespace Data.ViewModel
{
    public class SquareVM
    {
        public SquareState State { get; set; }
        public bool OpenRight { get; set; }
        public bool OpenBottom { get; set; }
        public bool OpenLeft { get; set; }
        public bool OpenTop { get; set; }
    }
}
