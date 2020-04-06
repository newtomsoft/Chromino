using Data;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Tool
{
    public class PictureFactoryTool
    {
        const string DefaultFile = "DefaultPicture.png";
        private string ImageGamePath { get; }

        /// <summary>
        /// Id du jeu
        /// </summary>
        private int GameId { get; set; }
        private GameDal GameDal { get; }
        private SquareDal SquareDal { get; }

        public PictureFactoryTool(int gameId, string imagePath, Context ctx)
        {
            GameDal = new GameDal(ctx);
            SquareDal = new SquareDal(ctx);
            GameId = gameId;
            ImageGamePath = imagePath;
        }

        /// <summary>
        /// créer le visuel du jeu
        /// </summary>
        public void MakeThumbnail()
        {
            //obtention des couleurs de différents carrés de l'image
            List<Square> squares = SquareDal.List(GameId);
            if (squares.Count == 0)
                return;

            int xMin = squares.Select(g => g.X).Min() - 1;
            int xMax = squares.Select(g => g.X).Max() + 1;
            int yMin = squares.Select(g => g.Y).Min() - 1;
            int yMax = squares.Select(g => g.Y).Max() + 1;
            int columnsNumber = xMax - xMin + 1;
            int linesNumber = yMax - yMin + 1;
            int squaresNumber = columnsNumber * linesNumber;
            var squaresViewModel = new SquareVM[squaresNumber];
            for (int i = 0; i < squaresViewModel.Length; i++)
                squaresViewModel[i] = new SquareVM(ColorCh.None, true, true, true, true);
            foreach (Square square in squares)
            {
                int index = square.Y * columnsNumber + square.X - (yMin * columnsNumber + xMin);
                squaresViewModel[index] = square.SquareViewModel;
            }

            //construction de l'image
            const int imageSquareSize = 32;
            int width = columnsNumber * imageSquareSize;
            int height = linesNumber * imageSquareSize;
            Bitmap thumbnail = new Bitmap(width, height);

            string cameleonFullFileName = Path.Combine(ImageGamePath, "../Cameleon.png");
            Bitmap cameleonBitmap = new Bitmap(cameleonFullFileName);
            if (cameleonBitmap.Width != imageSquareSize - 2)
                cameleonBitmap = new Bitmap(cameleonBitmap, new Size(imageSquareSize - 2, imageSquareSize - 2));

            for (int j = 0; j < linesNumber; j++)
            {
                for (int i = 0; i < columnsNumber; i++)
                {
                    bool firstCameleonPixel = true;
                    int index = i + j * columnsNumber;
                    ColorCh colorSquare = squaresViewModel[index].Color;
                    ColorCh colorSquareLeft = i != 0 ? squaresViewModel[i - 1 + j * columnsNumber].Color : ColorCh.None;
                    ColorCh colorSquareRight = i != columnsNumber - 1 ? squaresViewModel[i + 1 + j * columnsNumber].Color : ColorCh.None;
                    ColorCh colorSquareTop = j != 0 ? squaresViewModel[i + (j - 1) * columnsNumber].Color : ColorCh.None;
                    ColorCh colorSquareBottom = j != linesNumber - 1 ? squaresViewModel[i + (j + 1) * columnsNumber].Color : ColorCh.None;
                    bool openRight = squaresViewModel[index].OpenRight;
                    bool openBottom = squaresViewModel[index].OpenBottom;
                    bool openLeft = squaresViewModel[index].OpenLeft;
                    bool openTop = squaresViewModel[index].OpenTop;
                    for (int x = i * imageSquareSize; x < (i + 1) * imageSquareSize; x++)
                    {
                        for (int y = j * imageSquareSize; y < (j + 1) * imageSquareSize; y++)
                        {
                            if (x == i * imageSquareSize)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail.SetPixel(x, y, openLeft ? Color.Gray : Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareLeft == ColorCh.None ? Color.Transparent : Color.Black);
                            }
                            else if (x == (i + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail.SetPixel(x, y, openRight ? Color.Gray : Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareRight == ColorCh.None ? Color.Transparent : Color.Black);
                            }
                            else if (y == j * imageSquareSize)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail.SetPixel(x, y, openTop ? Color.Gray : Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareTop == ColorCh.None ? Color.Transparent : Color.Black);
                            }
                            else if (y == (j + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail.SetPixel(x, y, openBottom ? Color.Gray : Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareBottom == ColorCh.None ? Color.Transparent : Color.Black);
                            }
                            else
                            {
                                if (colorSquare == ColorCh.Cameleon)
                                {
                                    if (firstCameleonPixel)
                                    {
                                        firstCameleonPixel = false;
                                        CopyPasteCameleonBitmap(ref thumbnail, ref cameleonBitmap, x, y);
                                    }
                                }
                                else
                                {
                                    thumbnail.SetPixel(x, y, EnumColorToColor(colorSquare));
                                }
                            }
                        }
                    }
                }
            }
            string thumbnailFullName = Path.Combine(ImageGamePath, $"{GameDal.Details(GameId).Guid}.png");

            // augmentation de la taille du canvas si pas assez de squares dans l'image
            const int minColumnsDisplayed = 15;
            const int minLinesDisplayed = minColumnsDisplayed;
            if (columnsNumber < minColumnsDisplayed || linesNumber < minLinesDisplayed)
            {
                int newWidth = Math.Max(minColumnsDisplayed * imageSquareSize, width);
                int newHeight = Math.Max(minLinesDisplayed * imageSquareSize, height);
                Bitmap resizedthumbnail = new Bitmap(newWidth, newHeight);
                Graphics graphics = Graphics.FromImage(resizedthumbnail);
                graphics.FillRectangle(Brushes.Transparent, 0, 0, newWidth, newHeight);
                graphics.DrawImage(thumbnail, (newWidth - thumbnail.Width) / 2, (newHeight - thumbnail.Height) / 2, thumbnail.Width, thumbnail.Height);
                thumbnail = new Bitmap(resizedthumbnail, newWidth / 2, newHeight / 2);
            }
            else
            {
                thumbnail = new Bitmap(thumbnail, width / 2, height / 2);
            }

            try
            {
                thumbnail.Save(thumbnailFullName, ImageFormat.Png);
            }
            catch
            {
                if (!File.Exists(thumbnailFullName))
                    File.Copy(Path.Combine(ImageGamePath, DefaultFile), thumbnailFullName);
            }
        }

        /// <summary>
        /// convertit le Data.Enumeration.Color en Color
        /// </summary>
        /// <param name="color">couleur en Data.Enumeration.Color</param>
        /// <returns>couleur en Color</returns>
        private Color EnumColorToColor(ColorCh color)
        {
            return color switch
            {
                ColorCh.Blue => Color.FromArgb(58, 194, 238),
                ColorCh.Green => Color.FromArgb(76, 174, 68),
                ColorCh.Purple => Color.FromArgb(86, 27, 108),
                ColorCh.Red => Color.FromArgb(250, 44, 46),
                ColorCh.Yellow => Color.FromArgb(255, 235, 71),
                ColorCh.Cameleon => Color.AntiqueWhite,
                _ => Color.Transparent,
            };
        }

        /// <summary>
        /// rempli la zone correspondant à un caméléon par l'image du caméléon
        /// </summary>
        /// <param name="thumbnail">bitmap de la vignette du jeu</param>
        /// <param name="bitmapCameleon">bitmap du caméléon</param>
        /// <param name="x">coordonnée x du coin supérieur gauche de la zone à remplir</param>
        /// <param name="y">coordonnée y du coin supérieur gauche de la zone à remplir</param>
        private void CopyPasteCameleonBitmap(ref Bitmap thumbnail, ref Bitmap bitmapCameleon, int x, int y)
        {
            for (int j = 0; j < bitmapCameleon.Height; j++)
                for (int i = 0; i < bitmapCameleon.Width; i++)
                    thumbnail.SetPixel(x + i, y + j, bitmapCameleon.GetPixel(i, j));
        }
    }
}
