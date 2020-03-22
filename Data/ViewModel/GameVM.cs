using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.ViewModel
{
    public class GameVM
    {
        public List<Square> Squares { get; set; }
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public int LinesNumber { get; set; }
        public int ColumnsNumber { get; set; }
        public int SquaresNumber { get; set; }
        public SquareVM[] SquaresViewModel { get; set; }
        public int ChrominosInStack { get; set; }
        public Dictionary<string, int> PseudosChrominos { get; set; }
        public Dictionary<string, ChrominoVM> Pseudos_LastChrominoVM { get; set; }
        public List<ChrominoVM> IdentifiedPlayerChrominosVM { get; set; }
        public Player PlayerTurn { get; set; }
        public Game Game { get; set; }
        public GamePlayer GamePlayerTurn { get; set; }
        public GamePlayer GamePlayerIdentified { get; set; }
        public List<int> BotsId { get; set; }
        public List<ChrominoPlayedVM> ChrominosPlayedVM { get; set; }
        public List<string> Pseudos { get; set; }
        public bool OpponenentsAreBots { get; set; }
        public bool NoTips { get; set; }

        public GameVM(Game game, List<Square> squares, int chrominosInStackNumber, Dictionary<string, int> pseudosChrominos, List<Chromino> identifiedPlayerChrominos, Player playerTurn, GamePlayer gamePlayerTurn, GamePlayer gamePlayerIdentified, List<int> botsId, Dictionary<string, Chromino> pseudos_lastChrominos, List<ChrominoInGame> chrominosInGamePlayed, List<string> pseudos, bool opponenentsAreBots, bool noTips)
        {
            OpponenentsAreBots = opponenentsAreBots;
            Game = game;
            Pseudos = pseudos;
            PlayerTurn = playerTurn;
            GamePlayerTurn = gamePlayerTurn;
            GamePlayerIdentified = gamePlayerIdentified;
            NoTips = noTips;
            ChrominosInStack = chrominosInStackNumber;
            PseudosChrominos = pseudosChrominos;
            Squares = squares;
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
                SquaresViewModel[i] = new SquareVM(Color.None, true, true, true, true);
            foreach (Square square in Squares)
            {
                int index = IndexGridState(square.X, square.Y);
                SquaresViewModel[index] = square.SquareViewModel;
            }

            IdentifiedPlayerChrominosVM = new List<ChrominoVM>();
            foreach (Chromino chromino in identifiedPlayerChrominos)
            {
                SquareVM square1 = new SquareVM(chromino.FirstColor, true, false, false, false);
                SquareVM square2 = new SquareVM(chromino.SecondColor, true, false, true, false);
                SquareVM square3 = new SquareVM(chromino.ThirdColor, true, false, true, false);
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
                SquareVM square1 = new SquareVM(pseudo_chromino.Value.FirstColor, true, false, false, false);
                SquareVM square2 = new SquareVM(pseudo_chromino.Value.SecondColor, true, false, true, false);
                SquareVM square3 = new SquareVM(pseudo_chromino.Value.ThirdColor, true, false, true, false);
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
        }

        private int IndexGridState(int x, int y)
        {
            if (x < XMin || x > XMax || y < YMin || y > YMax)
                throw new IndexOutOfRangeException();

            return y * ColumnsNumber + x - (YMin * ColumnsNumber + XMin);
        }
    }
}