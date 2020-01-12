using Data.Core;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Data.ViewModel
{
    public class GameViewModel
    {
        public int GameId { get; set; }
        public bool AutoPlay { get; set; }
        public List<Square> Squares { get; set; }
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public int LinesNumber { get; set; }
        public int ColumnsNumber { get; set; }
        public int SquaresNumber { get; set; }
        public SquareViewModel[] SquaresViewModel { get; set; }
        public int ChrominosInGame { get; set; }
        public int ChrominosInStack { get; set; }
        public List<int> PlayerNumberChrominos { get; set; }
        public GameStatus GameStatus { get; set; }

        public GameViewModel(int gameId, List<Square> squares, bool autoPlay, GameStatus gameStatus, int chrominosInGame, int chrominosInStack, List<int> playerNumberChrominos)
        {
            AutoPlay = autoPlay;
            GameId = gameId;
            ChrominosInGame = chrominosInGame;
            ChrominosInStack = chrominosInStack;
            PlayerNumberChrominos = playerNumberChrominos;
            Squares = squares;
            GameStatus = gameStatus;
            XMin = squares.Select(g => g.X).Min();
            XMax = squares.Select(g => g.X).Max();
            YMin = squares.Select(g => g.Y).Min();
            YMax = squares.Select(g => g.Y).Max();

            ColumnsNumber = XMax - XMin + 1;
            LinesNumber = YMax - YMin + 1;

            SquaresNumber = ColumnsNumber * LinesNumber;

            SquaresViewModel = new SquareViewModel[SquaresNumber];
            for (int i = 0; i < SquaresViewModel.Length; i++)
            {
                SquaresViewModel[i] = new SquareViewModel
                {
                    State = SquareViewModelState.Free,
                    Edge = OpenEdge.All,
                };
            }

            foreach (Square square in Squares)
            {
                int index = IndexGridState(square.X, square.Y);
                SquaresViewModel[index] = square.SquareViewModel;
            }
        }


        public int IndexGridState(int x, int y)
        {
            if (x < XMin || x > XMax || y < YMin || y > YMax)
                throw new IndexOutOfRangeException();

            return y * ColumnsNumber + x - (YMin * ColumnsNumber + XMin);
        }

        

    }


}
