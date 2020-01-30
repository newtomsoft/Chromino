using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel
{
    public class GameForListVM
    {
        public int GameId { get; set; }
        public string PlayerPseudoTurn { get; set; }
        public GameStatus GameStatus { get; set; }
        private Dictionary<string, int> Pseudos_Chrominos { get; set; }
        private DateTime PlayedDate { get; set; }
        public string Infos
        {
            get
            {
                StringBuilder infos = new StringBuilder();
                foreach (var pseudo_Chromino in Pseudos_Chrominos)
                {
                    infos.Append($"{pseudo_Chromino.Key} ({pseudo_Chromino.Value}) - ");
                }
                infos.Append("last played : " + PlayedDate);
                return infos.ToString();
            }
        }

        public GameForListVM(Game game, Dictionary<string, int> pseudos_chrominos, string playerPseudoTurn)
        {
            GameId = game.Id;
            GameStatus = game.Status;
            PlayedDate = game.PlayedDate;
            Pseudos_Chrominos = pseudos_chrominos;
            PlayerPseudoTurn = playerPseudoTurn;
        }
    }
}
