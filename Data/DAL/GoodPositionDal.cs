using Data.Core;
using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class GoodPositionDal
    {
        private readonly Context Ctx;

        public GoodPositionDal(Context context)
        {
            Ctx = context;
        }

        /// <summary>
        /// Supprime les ComputedChrominos qui ne sont potentiellement plus valables
        /// </summary>
        /// <param name="gameId">id du jeu concerné</param>
        /// <param name="playerId">id du joueur concerné</param>
        /// <param name="positions">liste des positions concernées</param>
        /// <param name="chrominoId">id du chromino concerné sinon tous les id</param>
        public void Remove(int gameId, int playerId, List<Position> positions, int chrominoId = 0)
        {
            foreach (var position in positions)
            {
                List<GoodPosition> toRemove = (from cc in Ctx.GoodPositions
                                               where cc.GameId == gameId && (cc.PlayerId == playerId || playerId == 0) && cc.X == position.Coordinate.X && cc.Y == position.Coordinate.Y && (cc.Orientation == position.Orientation) && (chrominoId == 0 || cc.ChrominoId == chrominoId)
                                               select cc).ToList();

                if (toRemove.Count > 0)
                    Ctx.GoodPositions.RemoveRange(toRemove);
            }
            Ctx.SaveChanges();
        }

        public void Remove(int gameId, int? botId, int chrominoId)
        {
            List<GoodPosition> gpToRemove = (from cc in Ctx.GoodPositions
                                             where cc.GameId == gameId && cc.PlayerId == botId && cc.ChrominoId == chrominoId
                                             select cc).ToList();

            if (gpToRemove.Count > 0)
            {
                Ctx.GoodPositions.RemoveRange(gpToRemove);
                Ctx.SaveChanges();
            }
        }

        public void Remove(int gameId, int botId)
        {
            List<GoodPosition> gpToRemove = (from cc in Ctx.GoodPositions
                                             where cc.GameId == gameId && cc.PlayerId == botId
                                             select cc).ToList();

            if (gpToRemove.Count > 0)
                Ctx.GoodPositions.RemoveRange(gpToRemove);

            Ctx.SaveChanges();
        }

        public void Add(HashSet<GoodPosition> goodPositions)
        {
            Ctx.GoodPositions.AddRange(goodPositions);
            Ctx.SaveChanges();
        }

        /// <summary>
        /// renvoie la liste des chrominos pouvant être joué au tour actuel classé par priorité
        /// </summary>
        /// <param name="gameId">Id de la partie</param>
        /// <param name="botId">Id du bot joueur</param>
        /// <param name="chominoIdNotToPlay">Id du chromino ne devant pas être joué</param>
        /// <returns></returns>
        public List<GoodPosition> RootListByPriority(int gameId, int botId, int chominoIdNotToPlay)
        {
            List<GoodPosition> ComputedChrominos = (from cc in Ctx.GoodPositions
                                                    join c in Ctx.Chrominos on cc.ChrominoId equals c.Id
                                                    where cc.GameId == gameId && cc.PlayerId == botId && cc.ParentId == null && cc.ChrominoId != chominoIdNotToPlay
                                                    orderby c.Points, c.SecondColor == ColorCh.Cameleon, c.Id
                                                    select cc).AsNoTracking().ToList();

            return ComputedChrominos;
        }

        public object Delete(IQueryable<int> gamesIdToDelete)
        {
            var resultGp = from gp in Ctx.GoodPositions
                         where gamesIdToDelete.Contains(gp.GameId)
                         select gp;

            var resultGpl = from gpl in Ctx.GoodPositionsLevel
                         where gamesIdToDelete.Contains(gpl.GameId)
                         select gpl;

            Ctx.GoodPositions.RemoveRange(resultGp);
            Ctx.GoodPositionsLevel.RemoveRange(resultGpl);
            return Ctx.SaveChanges();
        }
    }
}
