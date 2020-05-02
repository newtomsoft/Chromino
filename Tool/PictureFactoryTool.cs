using Data;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tool
{
    public class PictureFactoryTool
    {
        const string DefaultFile = "DefaultPicture.png";
        private string ImageGamePath { get; }
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

        public void MakeThumbnail()
        {
            const int minColumnsDisplayed = 15;
            const int minLinesDisplayed = minColumnsDisplayed;

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
            int offsetX = 0;
            int offsetY = 0;
            if (columnsNumber < minColumnsDisplayed)
            {
                offsetX = (int)Math.Ceiling((minColumnsDisplayed - columnsNumber) / 2.0);
                columnsNumber += 2 * offsetX;
                offsetY = (int)Math.Ceiling((minLinesDisplayed - linesNumber) / 2.0);
                linesNumber += 2 * offsetY;
            }
            int squaresNumber = columnsNumber * linesNumber;
            var squaresToDraw = new Square[squaresNumber];
            for (int i = 0; i < squaresToDraw.Length; i++)
                squaresToDraw[i] = new Square { Color = ColorCh.None, OpenRight = true, OpenBottom = true, OpenLeft = true, OpenTop = true };
            foreach (Square square in squares)
            {
                int index = (square.Y + offsetY) * columnsNumber + square.X + offsetX - (yMin * columnsNumber + xMin);
                squaresToDraw[index] = square;
            }

            //construction de l'image
            const int imageSquareSize = 32;
            int width = columnsNumber * imageSquareSize;
            int height = linesNumber * imageSquareSize;
            Image<Rgba32> thumbnail = new Image<Rgba32>(width, height);
            string cameleonFullFileName = Path.Combine(ImageGamePath, "../Cameleon.png");
            Image<Rgba32> cameleonImage = Image.Load(cameleonFullFileName).CloneAs<Rgba32>();
            if (cameleonImage.Width != imageSquareSize - 2)
                cameleonImage.Mutate(x => x.Resize(imageSquareSize - 2, imageSquareSize - 2));
            for (int j = offsetY; j < linesNumber - offsetY; j++)
            {
                for (int i = offsetX; i < columnsNumber - offsetX; i++)
                {
                    bool firstCameleonPixel = true;
                    int index = i + j * columnsNumber;
                    ColorCh colorSquare = squaresToDraw[index].Color;
                    ColorCh colorSquareLeft = i != 0 ? squaresToDraw[i - 1 + j * columnsNumber].Color : ColorCh.None;
                    ColorCh colorSquareRight = i != columnsNumber - 1 ? squaresToDraw[i + 1 + j * columnsNumber].Color : ColorCh.None;
                    ColorCh colorSquareTop = j != 0 ? squaresToDraw[i + (j - 1) * columnsNumber].Color : ColorCh.None;
                    ColorCh colorSquareBottom = j != linesNumber - 1 ? squaresToDraw[i + (j + 1) * columnsNumber].Color : ColorCh.None;
                    bool openRight = squaresToDraw[index].OpenRight;
                    bool openBottom = squaresToDraw[index].OpenBottom;
                    bool openLeft = squaresToDraw[index].OpenLeft;
                    bool openTop = squaresToDraw[index].OpenTop;
                    for (int x = i * imageSquareSize; x < (i + 1) * imageSquareSize; x++)
                    {
                        for (int y = j * imageSquareSize; y < (j + 1) * imageSquareSize; y++)
                        {
                            if (x == i * imageSquareSize)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail[x, y] = openLeft ? Color.Gray : Color.Black;
                                else
                                    thumbnail[x, y] = colorSquareLeft == ColorCh.None ? Color.Transparent : Color.Black;
                            }
                            else if (x == (i + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail[x, y] = openRight ? Color.Gray : Color.Black;
                                else
                                    thumbnail[x, y] = colorSquareRight == ColorCh.None ? Color.Transparent : Color.Black;
                            }
                            else if (y == j * imageSquareSize)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail[x, y] = openTop ? Color.Gray : Color.Black;
                                else
                                    thumbnail[x, y] = colorSquareTop == ColorCh.None ? Color.Transparent : Color.Black;
                            }
                            else if (y == (j + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != ColorCh.None)
                                    thumbnail[x, y] = openBottom ? Color.Gray : Color.Black;
                                else
                                    thumbnail[x, y] = colorSquareBottom == ColorCh.None ? Color.Transparent : Color.Black;
                            }
                            else
                            {
                                if (colorSquare == ColorCh.Cameleon)
                                {
                                    if (firstCameleonPixel)
                                    {
                                        firstCameleonPixel = false;
                                        CopyPasteCameleonBitmap(ref thumbnail, ref cameleonImage, x, y);
                                    }
                                }
                                else
                                {
                                    thumbnail[x, y] = ColorChToColor(colorSquare);
                                }
                            }
                        }
                    }
                }
            }
            string thumbnailFullName = Path.Combine(ImageGamePath, $"{GameDal.Details(GameId).Guid}.png");

            thumbnail.Mutate(x => x.Resize(thumbnail.Width / 2, thumbnail.Height / 2));
            try
            {
                thumbnail.Save(thumbnailFullName);
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
        private Color ColorChToColor(ColorCh color)
        {
            return color switch
            {
                ColorCh.Blue => Color.FromRgb(58, 194, 238),
                ColorCh.Green => Color.FromRgb(76, 174, 68),
                ColorCh.Purple => Color.FromRgb(86, 27, 108),
                ColorCh.Red => Color.FromRgb(250, 44, 46),
                ColorCh.Yellow => Color.FromRgb(255, 235, 71),
                _ => Color.Transparent,
            };
        }

        /// <summary>
        /// rempli la zone correspondant à un caméléon par l'image du caméléon
        /// </summary>
        /// <param name="thumbnail">image de la vignette du jeu</param>
        /// <param name="bitmapCameleon">image du caméléon</param>
        /// <param name="x">coordonnée x du coin supérieur gauche de la zone à remplir</param>
        /// <param name="y">coordonnée y du coin supérieur gauche de la zone à remplir</param>
        private void CopyPasteCameleonBitmap(ref Image<Rgba32> thumbnail, ref Image<Rgba32> bitmapCameleon, int x, int y)
        {
            for (int j = 0; j < bitmapCameleon.Height; j++)
                for (int i = 0; i < bitmapCameleon.Width; i++)
                    thumbnail[x + i, y + j] = bitmapCameleon[i, j];
        }
    }
}
