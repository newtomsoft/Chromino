#if DEBUG

using Data.Models;
using System.Collections.Generic;

namespace Data.DAL
{
    public partial class SquareDal
    {
        public void AddTestDebug(List<Square> squares, int gameId)
        {
            foreach (var square in squares)
            {
                square.Id = 0;
                square.GameId = gameId;
            }
            Ctx.Squares.AddRange(squares);
            Ctx.SaveChanges();
        }
    }
}

#endif