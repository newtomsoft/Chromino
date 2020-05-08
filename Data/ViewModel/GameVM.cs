using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.ViewModel
{
    public class GameVM
    {
        public Player Player { get; set; }
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public int LinesNumber { get; set; }
        public int ColumnsNumber { get; set; }
        public int SquaresNumber { get; set; }
        public Square[] Squares { get; set; }
        public int ChrominosInStack { get; set; }
        public Dictionary<string, int> PseudosChrominos { get; set; }
        public Dictionary<string, int> PseudosIds { get; set; }
        public Dictionary<string, ChrominoVM> Pseudos_LastChrominoVM { get; set; }
        public List<ChrominoVM> PlayerChrominosVM { get; set; }
        public Player PlayerTurn { get; set; }
        public Game Game { get; set; }
        public GamePlayer GamePlayer { get; set; }
        public List<ChrominoPlayedVM> ChrominosPlayedVM { get; set; }
        public List<string> Pseudos { get; set; }
        public List<Tip> Tips { get; set; }
        public List<PlayError> PlayErrors { get; set; }

        public GameVM(Game game, Player player, List<Square> squares, int chrominosInStackNumber, Dictionary<string, int> pseudosChrominos, Dictionary<string, int> pseudosIds, List<Chromino> playerChrominos, Player playerTurn, GamePlayer gamePlayer, Dictionary<string, Chromino> pseudos_lastChrominos, List<ChrominoInGame> chrominosInGamePlayed, List<string> pseudos, List<Tip> tipsOn, List<PlayError> playErrors)
        {
            Player = player;
            Game = game;
            PlayerTurn = playerTurn;
            GamePlayer = gamePlayer;
            ChrominosInStack = chrominosInStackNumber;
            XMin = squares.Select(g => g.X).Min() - 2; // +- 2 pour marge permettant de poser un chromino sur un bord
            XMax = squares.Select(g => g.X).Max() + 2;
            YMin = squares.Select(g => g.Y).Min() - 2;
            YMax = squares.Select(g => g.Y).Max() + 2;
            ColumnsNumber = XMax - XMin + 1;
            LinesNumber = YMax - YMin + 1;
            SquaresNumber = ColumnsNumber * LinesNumber;
            Squares = new Square[SquaresNumber];
            for (int i = 0; i < Squares.Length; i++)
                Squares[i] = new Square { Color = ColorCh.None };
            foreach (Square square in squares)
                Squares[IndexGridState(square.X, square.Y)] = square;

            PlayerChrominosVM = new List<ChrominoVM>();
            foreach (Chromino chromino in playerChrominos)
                PlayerChrominosVM.Add(new ChrominoVM { ChrominoId = chromino.Id, Squares = new Square[3] { new Square { Color = chromino.FirstColor }, new Square { Color = chromino.SecondColor }, new Square { Color = chromino.ThirdColor } } });

            Pseudos_LastChrominoVM = new Dictionary<string, ChrominoVM>();
            foreach (var pseudo_chromino in pseudos_lastChrominos)
                Pseudos_LastChrominoVM.Add(pseudo_chromino.Key != Player.UserName ? pseudo_chromino.Key : "Vous", new ChrominoVM { ChrominoId = pseudo_chromino.Value.Id, Squares = new Square[3] { new Square { Color = pseudo_chromino.Value.FirstColor, OpenRight = true }, new Square { Color = pseudo_chromino.Value.SecondColor, OpenRight = true, OpenLeft = true }, new Square { Color = pseudo_chromino.Value.ThirdColor, OpenLeft = true } } });

            ChrominosPlayedVM = new List<ChrominoPlayedVM>();
            foreach (ChrominoInGame chrominoInGame in chrominosInGamePlayed)
                ChrominosPlayedVM.Add(new ChrominoPlayedVM(chrominoInGame, XMin, YMin));

            Pseudos = pseudos;
            PseudosChrominos = pseudosChrominos;
            PseudosIds = pseudosIds;
            int indexPlayerPseudo = Pseudos.IndexOf(Player.UserName);
            if (indexPlayerPseudo != -1)
            {
                Pseudos[indexPlayerPseudo] = "Vous";
                int value = PseudosChrominos[Player.UserName];
                PseudosChrominos.Remove(Player.UserName);
                PseudosChrominos["Vous"] = value;
                value = PseudosIds[Player.UserName];
                PseudosIds.Remove(Player.UserName);
                PseudosIds["Vous"] = value;
            }

            Tips = tipsOn;
            PlayErrors = playErrors;
        }

        private int IndexGridState(int x, int y)
        {
            if (x < XMin || x > XMax || y < YMin || y > YMax)
                throw new IndexOutOfRangeException();

            return y * ColumnsNumber + x - (YMin * ColumnsNumber + XMin);
        }
    }
}