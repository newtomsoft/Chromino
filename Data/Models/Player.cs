using Microsoft.AspNetCore.Identity;

namespace Data.Models
{
    public class Player : IdentityUser<int>
    {
        override public string UserName { get; set; }
        public int Points { get; set; }
        public int WonGames { get; set; }
        public int SinglePlayerGamesFinished { get; set; }
        public int SinglePlayerGamesPoints { get; set; }
        public bool Bot { get; set; }
        public int Help { get; set; }

#if DEBUG
        static public int InitHelp = 99;
#else
        static public int InitHelp = 5;
#endif
    }
}
