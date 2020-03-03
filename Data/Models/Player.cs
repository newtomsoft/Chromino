using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    public class Player : IdentityUser <int>
    {
        override public string UserName { get; set; }
        public int PlayedGames { get; set; }
        public int Points { get; set; }
        public int WonGames { get; set; }
        public int GamesFinished { get; set; }
        public int SinglePlayerGamesFinished { get; set; }
        public int SinglePlayerGamesPoints { get; set; }
        public bool Bot { get; set; }
    }
}
