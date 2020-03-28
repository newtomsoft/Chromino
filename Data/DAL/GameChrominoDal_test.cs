using Data.Core;
using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Data.DAL
{
    public partial class GameChrominoDal
    {
        public ChrominoInGame FirstTestToGame(int gameId)
        {
            Chromino chromino = (from c in Ctx.Chrominos
                                 where c.Id == 1
                                 select c).AsNoTracking().FirstOrDefault();

            Orientation orientation = (Orientation)Random.Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);
            Coordinate offset = new Coordinate(orientation);
            Coordinate coordinate = new Coordinate(0, 0) - offset;
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chromino.Id,
                XPosition = coordinate.X,
                YPosition = coordinate.Y,
                Orientation = orientation,
            };
            Ctx.ChrominosInGame.Add(chrominoInGame);
            Ctx.SaveChanges();
            return chrominoInGame;
        }

        public int StackTestToHand(int gameId, int playerId, int chromonoId)
        {
            ChrominoInHand chrominoInHand = new ChrominoInHand()
            {
                PlayerId = playerId,
                GameId = gameId,
                ChrominoId = chromonoId,
                Position = 1,
            };
            Ctx.ChrominosInHand.Add(chrominoInHand);
            Ctx.SaveChanges();
            return chromonoId;
        }


    }
}
