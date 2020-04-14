using Data.Enumeration;
using Data.Models;

namespace Data.ViewModel
{
    public class PossiblesChrominoVM
    {
        public short[] IndexesX { get; set; } = new short[3];
        public short[] IndexesY { get; set; } = new short[3];

        public PossiblesChrominoVM(GoodPosition goodPosition, int xMin, int yMin)
        {
            int indexX = goodPosition.X - xMin;
            int indexY = goodPosition.Y - yMin;
            IndexesX[0] = (short)indexX;
            IndexesY[0] = (short)indexY;
            switch (goodPosition.Orientation)
            {
                case Orientation.Horizontal:
                    IndexesX[1] = (short)(indexX + 1);
                    IndexesY[1] = (short)indexY;
                    IndexesX[2] = (short)(indexX + 2);
                    IndexesY[2] = (short)indexY;
                    break;
                case Orientation.Vertical:
                    IndexesX[1] = (short)indexX;
                    IndexesY[1] = (short)(indexY + 1);
                    IndexesX[2] = (short)indexX;
                    IndexesY[2] = (short)(indexY + 2);
                    break;
            }
        }
    }
}
