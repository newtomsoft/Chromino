using Data.Enumeration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel
{
    public class SquareVM
    {
        public SquareVMState State { get; set; }
        //public OpenEdge Edge { get; set; }
        public bool OpenRight { get; set; }
        public bool OpenBottom { get; set; }
        public bool OpenLeft { get; set; }
        public bool OpenTop { get; set; }
    }
}
