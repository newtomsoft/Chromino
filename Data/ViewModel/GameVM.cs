using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.ViewModel
{
    public class GameVM
    {
        public string PlayerPseudo { get; set; }
        public int PlayerId { get; set; }
        public List<Square> Squares { get; set; }
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public int LinesNumber { get; set; }
        public int ColumnsNumber { get; set; }
        public int SquaresNumber { get; set; }
        public SquareVM[] SquaresVM { get; set; }
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

        public GameVM(Game game, string playerPseudo, int playerId, List<Square> squares, int chrominosInStackNumber, Dictionary<string, int> pseudosChrominos, List<Chromino> identifiedPlayerChrominos, Player playerTurn, GamePlayer gamePlayerTurn, GamePlayer gamePlayerIdentified, List<int> botsId, Dictionary<string, Chromino> pseudos_lastChrominos, List<ChrominoInGame> chrominosInGamePlayed, List<string> pseudos, bool opponenentsAreBots, bool noTips)
        {
            PlayerPseudo = playerPseudo;
            PlayerId = playerId;
            OpponenentsAreBots = opponenentsAreBots;
            Game = game;
            PlayerTurn = playerTurn;
            GamePlayerTurn = gamePlayerTurn;
            GamePlayerIdentified = gamePlayerIdentified;
            NoTips = noTips;
            ChrominosInStack = chrominosInStackNumber;
            Squares = squares;
            BotsId = botsId;
            XMin = squares.Select(g => g.X).Min() - 1; // +- 1 pour marge permettant de poser un chromino sur un bord
            XMax = squares.Select(g => g.X).Max() + 1;
            YMin = squares.Select(g => g.Y).Min() - 1;
            YMax = squares.Select(g => g.Y).Max() + 1;
            ColumnsNumber = XMax - XMin + 1;
            LinesNumber = YMax - YMin + 1;
            SquaresNumber = ColumnsNumber * LinesNumber;
            SquaresVM = new SquareVM[SquaresNumber];
            for (int i = 0; i < SquaresVM.Length; i++)
                SquaresVM[i] = new SquareVM(ColorCh.None);
            foreach (Square square in Squares)
                SquaresVM[IndexGridState(square.X, square.Y)] = square.SquareViewModel;

            IdentifiedPlayerChrominosVM = new List<ChrominoVM>();
            foreach (Chromino chromino in identifiedPlayerChrominos)
                IdentifiedPlayerChrominosVM.Add(new ChrominoVM() { ChrominoId = chromino.Id, SquaresVM = new SquareVM[3] { new SquareVM(chromino.FirstColor), new SquareVM(chromino.SecondColor), new SquareVM(chromino.ThirdColor) } });

            Pseudos_LastChrominoVM = new Dictionary<string, ChrominoVM>();
            foreach (var pseudo_chromino in pseudos_lastChrominos)
                Pseudos_LastChrominoVM.Add(pseudo_chromino.Key != playerPseudo ? pseudo_chromino.Key : "Vous", new ChrominoVM() { ChrominoId = pseudo_chromino.Value.Id, SquaresVM = new SquareVM[3] { new SquareVM(pseudo_chromino.Value.FirstColor, true, false, false, false), new SquareVM(pseudo_chromino.Value.SecondColor, true, false, true, false), new SquareVM(pseudo_chromino.Value.ThirdColor, true, false, true, false) } });

            ChrominosPlayedVM = new List<ChrominoPlayedVM>();
            foreach (ChrominoInGame chrominoInGame in chrominosInGamePlayed)
                ChrominosPlayedVM.Add(new ChrominoPlayedVM(chrominoInGame, XMin, YMin));

            Pseudos = pseudos;
            PseudosChrominos = pseudosChrominos;
            int indexPlayerPseudo = Pseudos.IndexOf(playerPseudo);
            if (indexPlayerPseudo != -1)
            {
                Pseudos[indexPlayerPseudo] = "Vous";
                int value = PseudosChrominos[playerPseudo];
                PseudosChrominos.Remove(playerPseudo);
                PseudosChrominos["Vous"] = value;
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