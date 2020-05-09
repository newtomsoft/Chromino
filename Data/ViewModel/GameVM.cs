using Data.Enumeration;
using Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Data.ViewModel
{
    public class GameVM
    {
        public Player Player { get; set; }
        public int XMin { get; set; }
        public int YMin { get; set; }
        public int LinesNumber { get; set; }
        public int ColumnsNumber { get; set; }
        public Square[] Squares { get; set; }
        public int ChrominosInStack { get; set; }
        public Dictionary<string, int> PseudosChrominos { get; set; }
        public Dictionary<string, int> PseudosIds { get; set; }
        public List<ChrominoVM> PlayerChrominosVM { get; set; }
        public Player PlayerTurn { get; set; }
        public Game Game { get; set; }
        public GamePlayer GamePlayer { get; set; }
        public List<ChrominoPlayedVM> ChrominosPlayedVM { get; set; }
        public List<string> Pseudos { get; set; }
        public List<Tip> Tips { get; set; }
        public List<PlayError> PlayErrors { get; set; }
        public bool IsGameFinish { get; set; }
        public bool HaveDrew { get; set; }
        public int MemosNumber { get; set; }

        public GameVM(Game game, Player player, List<Square> squares, int chrominosInStackNumber, Dictionary<string, int> pseudosChrominos, Dictionary<string, int> pseudosIds, List<Chromino> playerChrominos, Player playerTurn, GamePlayer gamePlayer, Dictionary<string, Chromino> pseudos_lastChrominos, List<ChrominoInGame> chrominosInGamePlayed, List<string> pseudos, List<Tip> tipsOn, List<PlayError> playErrors)
        {

            Player = player;
            Game = game;
            IsGameFinish = Game.Status.IsFinished();
            PlayerTurn = playerTurn;
            GamePlayer = gamePlayer;
            ChrominosInStack = chrominosInStackNumber;
            XMin = squares.Select(g => g.X).Min() - 2; // +- 2 pour marge permettant de poser un chromino sur un bord
            int xMax = squares.Select(g => g.X).Max() + 2;
            YMin = squares.Select(g => g.Y).Min() - 2;
            int yMax = squares.Select(g => g.Y).Max() + 2;
            ColumnsNumber = xMax - XMin + 1;
            LinesNumber = yMax - YMin + 1;
            int SquaresNumber = ColumnsNumber * LinesNumber;
            Squares = new Square[SquaresNumber];
            for (int i = 0; i < Squares.Length; i++)
                Squares[i] = new Square { Color = ColorCh.None };
            foreach (Square square in squares)
                Squares[IndexGridState(square.X, square.Y)] = square;

            PlayerChrominosVM = new List<ChrominoVM>();
            foreach (Chromino chromino in playerChrominos)
                PlayerChrominosVM.Add(new ChrominoVM { ChrominoId = chromino.Id, Squares = new Square[3] { new Square { Color = chromino.FirstColor }, new Square { Color = chromino.SecondColor }, new Square { Color = chromino.ThirdColor } } });

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
            HaveDrew = GamePlayer != null ? GamePlayer.PreviouslyDraw : false;
            MemosNumber = GamePlayer?.Memo?.Count(x => x == '\n') + 1 ?? 0;
        }

        private int IndexGridState(int x, int y) => y * ColumnsNumber + x - (YMin * ColumnsNumber + XMin);
    }
}