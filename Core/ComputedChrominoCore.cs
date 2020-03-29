using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;


namespace Core
{
    class ComputedChrominoCore : ComputedChromino
    {
        public static List<ComputedChromino> ToDelete(Square square)
        {
            int x = square.X;
            int y = square.Y;

            List<ComputedChromino> ComputedChrominos = new List<ComputedChromino>();
            foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
            {
                int coef;
                if (orientation == Orientation.Horizontal || orientation == Orientation.Vertical)
                    coef = 1;
                else
                    coef = -1;

                switch (orientation)
                {
                    case Orientation.Horizontal:
                    case Orientation.HorizontalFlip:
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 3, Y = y });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 2, Y = y - 1 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 2, Y = y });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 2, Y = y + 1 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef, Y = y - 1 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef, Y = y });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef, Y = y + 1 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y - 1 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + 1 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + coef, Y = y });
                        break;

                    case Orientation.Vertical:
                    case Orientation.VerticalFlip:
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + coef * 3 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - 1, Y = y + coef * 2 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + coef * 2 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + 1, Y = y + coef * 2 });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - 1, Y = y + coef });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + coef });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + 1, Y = y + coef });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - 1, Y = y });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + 1, Y = y });
                        ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y - coef });
                        break;
                }
            }
            return ComputedChrominos;
        }
    }
}
