using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public partial class ChrominoInHandLastDal
    {
        private readonly Context Ctx;

        public ChrominoInHandLastDal(Context context)
        {
            Ctx = context;
        }



        public int LastChrominoIdInHand(int gameId, int playerId)
        {
            int chrominoId = (from chl in Ctx.ChrominosInHandLast
                              where chl.GameId == gameId && chl.PlayerId == playerId
                              select chl.ChrominoId).FirstOrDefault();

            return chrominoId;
        }

        public void UpdateLastChrominoInHand(int gameId, int playerId, int chrominoId)
        {
            ChrominoInHandLast chrominohl = (from chl in Ctx.ChrominosInHandLast
                                             where chl.GameId == gameId && chl.PlayerId == playerId
                                             select chl).FirstOrDefault();

            if (chrominohl == null)
            {
                chrominohl = new ChrominoInHandLast { GameId = gameId, PlayerId = playerId, ChrominoId = chrominoId };
                Ctx.Add(chrominohl);
                Ctx.SaveChanges();
            }
            else if (chrominohl.ChrominoId != chrominoId)
            {
                chrominohl.ChrominoId = chrominoId;
                Ctx.SaveChanges();
            }
        }

        public void Delete(int gameId, int playerId)
        {
            ChrominoInHandLast chromino = (from chl in Ctx.ChrominosInHandLast
                                           where chl.GameId == gameId && chl.PlayerId == playerId
                                           select chl).FirstOrDefault();

            if (chromino != null)
                Ctx.ChrominosInHandLast.Remove(chromino);
            Ctx.SaveChanges();
        }

        public List<int> PlayersIdWithOneChrominoKnown(int gameId, int playerIdToExclude)
        {
            List<int> playersId = (from chl in Ctx.ChrominosInHandLast
                                   where chl.GameId == gameId && chl.PlayerId != playerIdToExclude
                                   select chl.PlayerId).ToList();

            return playersId;
        }

        public List<Chromino> ListLastChrominoIdInHand(int gameId, int playerIdToExclude)
        {
            List<Chromino> chrominos = (from chl in Ctx.ChrominosInHandLast
                                        join c in Ctx.Chrominos on chl.ChrominoId equals c.Id
                                        where chl.GameId == gameId && chl.PlayerId != playerIdToExclude
                                        select c).AsNoTracking().ToList();

            return chrominos;
        }

        public void Add(ChrominoInGame chrominoInGame)
        {
            chrominoInGame.Id = 0;
            Ctx.ChrominosInGame.Add(chrominoInGame);
            Ctx.SaveChanges();
        }
    }
}
