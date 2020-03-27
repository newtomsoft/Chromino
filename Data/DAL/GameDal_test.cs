#if DEBUG

using Data.Enumeration;
using Data.Models;
using System;

namespace Data.DAL
{
    public partial class GameDal
    {
        public Game AddTestDebug()
        {
            string guid = Guid.NewGuid().ToString("N");
            Game game = new Game { CreateDate = DateTime.Now, Guid = guid, Move = 0, Status = GameStatus.InProgress };
            Ctx.Games.Add(game);
            Ctx.SaveChanges();
            return Details(guid);
        }
    }
}

#endif