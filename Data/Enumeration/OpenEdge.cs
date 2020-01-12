using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Enumeration
{
    public enum OpenEdge
    {
        Right = 1,
        Bottom,
        Left,
        Top,
        RightLeft,
        BottomTop,
        All,
        BottomLeftTop,
        RightLeftTop,
        LeftTop,
        BottomLeft,
        RightTop,
    }


    public class Side
    {
        public OpenEdge OpenEdge { get; set; }



    }
}
