using Data.Core;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data.DAL;

namespace Data.ViewModel
{
    public class GameVM
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
        public SquareVM[] SquaresViewModel { get; set; }
        public int ChrominosInGame { get; set; }
        public int ChrominosInStack { get; set; }
        public Dictionary<string, int> Pseudos_Chrominos { get; set; }
        public Dictionary<string, ChrominoVM> Pseudos_LastChrominoVM { get; set; }
        public GameStatus GameStatus { get; set; }
        public List<ChrominoVM> IdentifiedPlayerChrominosVM { get; set; }
        public string PlayerPseudoTurn { get; set; }
        public int PlayerIdTurn { get; set; }
        public GamePlayer GamePlayerTurn { get; set; }
        public List<int> BotsId { get; set; }

        public GameVM(int gameId, List<Square> squares, bool autoPlay, GameStatus gameStatus, int chrominosInGame, int chrominosInStack, Dictionary<string, int> pseudos_chrominos, List<Chromino> identifiedPlayerChrominos, Player playerTurn, GamePlayer gamePlayerTurn, List<int> botsId, Dictionary<string, Chromino> pseudos_lastChrominos)
        {
            PlayerPseudoTurn = playerTurn.Pseudo;
            PlayerIdTurn = playerTurn.Id;
            GamePlayerTurn = gamePlayerTurn;
            AutoPlay = autoPlay;
            GameId = gameId;
            ChrominosInGame = chrominosInGame;
            ChrominosInStack = chrominosInStack;
            Pseudos_Chrominos = pseudos_chrominos;
            Squares = squares;
            GameStatus = gameStatus;
            BotsId = botsId;
            XMin = squares.Select(g => g.X).Min() - 1; // +- 1 pour marge permettant de poser un chromino sur un bord
            XMax = squares.Select(g => g.X).Max() + 1;
            YMin = squares.Select(g => g.Y).Min() - 1;
            YMax = squares.Select(g => g.Y).Max() + 1;

            ColumnsNumber = XMax - XMin + 1;
            LinesNumber = YMax - YMin + 1;

            SquaresNumber = ColumnsNumber * LinesNumber;

            SquaresViewModel = new SquareVM[SquaresNumber];
            for (int i = 0; i < SquaresViewModel.Length; i++)
            {
                SquaresViewModel[i] = new SquareVM
                {
                    State = SquareVMState.Free,
                    Edge = OpenEdge.All,
                };
            }

            foreach (Square square in Squares)
            {
                int index = IndexGridState(square.X, square.Y);
                SquaresViewModel[index] = square.SquareViewModel;
            }

            IdentifiedPlayerChrominosVM = new List<ChrominoVM>();
            foreach (Chromino chromino in identifiedPlayerChrominos)
            {
                SquareVM square1 = new SquareVM { State = (SquareVMState)chromino.FirstColor, Edge = OpenEdge.Right };
                SquareVM square2 = new SquareVM { State = (SquareVMState)chromino.SecondColor, Edge = OpenEdge.RightLeft };
                SquareVM square3 = new SquareVM { State = (SquareVMState)chromino.ThirdColor, Edge = OpenEdge.Left };
                ChrominoVM chrominoViewModel = new ChrominoVM();
                chrominoViewModel.SquaresViewModel[0] = square1;
                chrominoViewModel.SquaresViewModel[1] = square2;
                chrominoViewModel.SquaresViewModel[2] = square3;
                chrominoViewModel.ChrominoId = chromino.Id;
                IdentifiedPlayerChrominosVM.Add(chrominoViewModel);
                //TODO faire edge
            }

            Pseudos_LastChrominoVM = new Dictionary<string, ChrominoVM>();
            foreach (var pseudo_chromino in pseudos_lastChrominos)
            {
                SquareVM square1 = new SquareVM { State = (SquareVMState)pseudo_chromino.Value.FirstColor, Edge = OpenEdge.Right };
                SquareVM square2 = new SquareVM { State = (SquareVMState)pseudo_chromino.Value.SecondColor, Edge = OpenEdge.RightLeft };
                SquareVM square3 = new SquareVM { State = (SquareVMState)pseudo_chromino.Value.ThirdColor, Edge = OpenEdge.Left };
                ChrominoVM chrominoViewModel = new ChrominoVM();
                chrominoViewModel.SquaresViewModel[0] = square1;
                chrominoViewModel.SquaresViewModel[1] = square2;
                chrominoViewModel.SquaresViewModel[2] = square3;
                chrominoViewModel.ChrominoId = pseudo_chromino.Value.Id;
                Pseudos_LastChrominoVM.Add(pseudo_chromino.Key, chrominoViewModel);
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