using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;

namespace ChrominoBI
{
    public class ChrominoInGameBI
    {
        public static ChrominoInGame ChrominoInGameFrom(GoodPosition goodPosition)
        {
            return new ChrominoInGame
            {
                GameId = goodPosition.GameId,
                PlayerId = goodPosition.PlayerId,
                ChrominoId = goodPosition.ChrominoId,
                Orientation = goodPosition.Orientation,
                XPosition = goodPosition.X,
                YPosition = goodPosition.Y,
                Flip = goodPosition.Flip,
            };
        }

        private ChrominoInGame ChrominoInGame { get; set; }
        private int GameId { get; set; }

        /// <summary>
        /// les différentes Dal utilisées du context 
        /// </summary>
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;

        public ChrominoInGameBI(Context ctx, int gameId, ChrominoInGame chrominoInGame)
        {
            ChrominoInGame = chrominoInGame;
            GameId = gameId;
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
        }

        public List<Coordinate> ChrominoCoordinates()
        {
            Coordinate offset = new Coordinate(ChrominoInGame.Orientation);
            Coordinate first = new Coordinate(ChrominoInGame.XPosition, ChrominoInGame.YPosition);
            Coordinate second = first + offset;
            Coordinate third = second + offset;
            return new List<Coordinate> { first, second, third };
        }


        /// <summary>
        /// indique si le chromino est à une position valide
        /// </summary>
        /// <param name="chrominoInGame">Chromino à jouer</param>
        /// <param name="squaresInGame">squares occupés</param>
        /// <param name="playReturn">code PlayReturn d'erreur si retour false</param>
        /// <returns>true si le chromino est à une position valide</returns>
        public bool IsValidChriminoInGame(ref List<Square> squaresInGame, out PlayReturn playReturn)
        {
            Chromino chromino = ChrominoDal.Details(ChrominoInGame.ChrominoId);
            playReturn = PlayReturn.Ok;
            if (ChrominoInGame.PlayerId == null)
                return true;
            else
            {
                List<Coordinate> chrominoCoordinates = ChrominoCoordinates();
                if (!chrominoCoordinates[0].IsFree(squaresInGame) || !chrominoCoordinates[1].IsFree(squaresInGame) || !chrominoCoordinates[2].IsFree(squaresInGame))
                {
                    playReturn = PlayReturn.NotFree;
                }
                else
                {
                    int n0 = SquareDal.GetNumberSameColorsAround(GameId, chrominoCoordinates[0], ChrominoInGame.Flip ? chromino.ThirdColor : chromino.FirstColor);
                    int n1 = SquareDal.GetNumberSameColorsAround(GameId, chrominoCoordinates[1], chromino.SecondColor);
                    int n2 = SquareDal.GetNumberSameColorsAround(GameId, chrominoCoordinates[2], ChrominoInGame.Flip ? chromino.FirstColor : chromino.ThirdColor);
                    if (n0 == -1 || n1 == -1 || n2 == -1)
                        playReturn = PlayReturn.DifferentColorsAround;
                    if (n0 + n1 + n2 < 2)
                        playReturn = PlayReturn.NotMinTwoSameColors;
                }
                if (playReturn.IsError())
                    return false;
                else
                    return true;
            }
        }
        public List<Square> GetSquares()
        {
            int gameId = ChrominoInGame.GameId;
            int xOrigin = ChrominoInGame.XPosition;
            int yOrigin = ChrominoInGame.YPosition;
            Chromino chromino = ChrominoDal.Details(ChrominoInGame.ChrominoId);
            List<Square> squares = new List<Square>() { new Square { GameId = gameId, X = xOrigin, Y = yOrigin, Color = chromino.FirstColor } };
            switch (ChrominoInGame.Orientation)
            {
                case Orientation.Horizontal:
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 1, Y = yOrigin, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 2, Y = yOrigin, Color = chromino.ThirdColor });
                    break;
                case Orientation.Vertical:
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 1, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 2, Color = chromino.ThirdColor });
                    break;
            }
            return squares;
        }



    }
}
