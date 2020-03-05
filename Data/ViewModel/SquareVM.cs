﻿using Data.Enumeration;

namespace Data.ViewModel
{
    public class SquareVM
    {
        public Color Color { get; set; }
        public bool OpenRight { get; set; }
        public bool OpenBottom { get; set; }
        public bool OpenLeft { get; set; }
        public bool OpenTop { get; set; }

        public SquareVM(Color color, bool openRight, bool openBottom, bool openLeft, bool openTop)
        {
            Color = color;
            OpenRight = openRight;
            OpenBottom = openBottom;
            OpenLeft = openLeft;
            OpenTop = openTop;
        }
    }
}
