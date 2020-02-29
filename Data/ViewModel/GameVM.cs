using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.ViewModel
{
    public class GameVM
    {
        public int GameId { get; set; }
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
        public List<ChrominoPlayedVM> ChrominosPlayedVM { get; set; }
        public List<string> PseudoChrominosPlayedVM { get; set; }

        public GameVM(int gameId, List<Square> squares, GameStatus gameStatus, int chrominosInGameNumber, int chrominosInStackNumber, Dictionary<string, int> pseudos_chrominos, List<Chromino> identifiedPlayerChrominos, Player playerTurn, GamePlayer gamePlayerTurn, List<int> botsId, Dictionary<string, Chromino> pseudos_lastChrominos, List<ChrominoInGame> chrominosInGamePlayed, List<string> pseudoChrominosInGamePlayed)
        {
            PlayerPseudoTurn = playerTurn.Pseudo;
            PlayerIdTurn = playerTurn.Id;
            GamePlayerTurn = gamePlayerTurn;
            GameId = gameId;
            ChrominosInGame = chrominosInGameNumber;
            ChrominosInStack = chrominosInStackNumber;
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
                    Color = Color.None,
                    OpenRight = true,
                    OpenBottom = true,
                    OpenLeft = true,
                    OpenTop = true,
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
                SquareVM square1 = new SquareVM { Color = chromino.FirstColor, OpenRight = true };
                SquareVM square2 = new SquareVM { Color = chromino.SecondColor, OpenRight = true, OpenLeft = true };
                SquareVM square3 = new SquareVM { Color = chromino.ThirdColor, OpenLeft = true };
                ChrominoVM chrominoViewModel = new ChrominoVM();
                chrominoViewModel.SquaresViewModel[0] = square1;
                chrominoViewModel.SquaresViewModel[1] = square2;
                chrominoViewModel.SquaresViewModel[2] = square3;
                chrominoViewModel.ChrominoId = chromino.Id;
                IdentifiedPlayerChrominosVM.Add(chrominoViewModel);
            }

            Pseudos_LastChrominoVM = new Dictionary<string, ChrominoVM>();
            foreach (var pseudo_chromino in pseudos_lastChrominos)
            {
                SquareVM square1 = new SquareVM { Color = pseudo_chromino.Value.FirstColor, OpenRight = true };
                SquareVM square2 = new SquareVM { Color = pseudo_chromino.Value.SecondColor, OpenRight = true, OpenLeft = true };
                SquareVM square3 = new SquareVM { Color = pseudo_chromino.Value.ThirdColor, OpenLeft = true };
                ChrominoVM chrominoViewModel = new ChrominoVM();
                chrominoViewModel.SquaresViewModel[0] = square1;
                chrominoViewModel.SquaresViewModel[1] = square2;
                chrominoViewModel.SquaresViewModel[2] = square3;
                chrominoViewModel.ChrominoId = pseudo_chromino.Value.Id;
                Pseudos_LastChrominoVM.Add(pseudo_chromino.Key, chrominoViewModel);
            }

            ChrominosPlayedVM = new List<ChrominoPlayedVM>();
            foreach (ChrominoInGame chrominoInGame in chrominosInGamePlayed)
            {
                ChrominoPlayedVM chrominoPlayedVM = new ChrominoPlayedVM(chrominoInGame, XMin, YMin);
                ChrominosPlayedVM.Add(chrominoPlayedVM);
            }
            PseudoChrominosPlayedVM = pseudoChrominosInGamePlayed;
        }

        public int IndexGridState(int x, int y)
        {
            if (x < XMin || x > XMax || y < YMin || y > YMax)
                throw new IndexOutOfRangeException();

            return y * ColumnsNumber + x - (YMin * ColumnsNumber + XMin);
        }
    }
}