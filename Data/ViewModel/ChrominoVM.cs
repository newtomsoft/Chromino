using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel
{
    public class ChrominoVM
    {
        public SquareVM[] SquaresViewModel { get; set; } = new SquareVM[3];
        public int ChrominoId {get;set;}
    }
}
