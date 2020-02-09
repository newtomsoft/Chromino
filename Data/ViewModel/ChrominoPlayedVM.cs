using Data.Enumeration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel
{
    public class ChrominoPlayedVM
    {
        public short[] IndexesX { get; set; } = new short[3];
        public short[] IndexesY { get; set; } = new short[3];
        public int PlayerId { get; set; }
    }
}
