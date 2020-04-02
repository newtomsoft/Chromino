using Data.Core;
using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class ComputedChrominosDal
    {
        private readonly Context Ctx;

        public ComputedChrominosDal(Context context)
        {
            Ctx = context;
        }

        /// <summary>
        /// Supprime les ComputedChrominos qui ne sont potentiellement plus valables
        /// </summary>
        /// <param name="gameId">id du jeu concerné</param>
        /// <param name="botId">id du bot concerné</param>
        /// <param name="positions">liste des positions concernées</param>
        /// <param name="chrominoId">id du chromino concerné sinon tous les id</param>
        public void Remove(int gameId, int botId, List<Position> positions, int chrominoId = 0)
        {
            foreach (var position in positions)
            {
                List<ComputedChromino> toRemove = (from cc in Ctx.ComputedChrominos
                                                   where cc.GameId == gameId && cc.PlayerId == botId && cc.X == position.Coordinate.X && cc.Y == position.Coordinate.Y && (cc.Orientation == position.Orientation) && (chrominoId == 0 || cc.ChrominoId == chrominoId)
                                                   select cc).ToList();

                if (toRemove != null)
                    Ctx.ComputedChrominos.RemoveRange(toRemove);
            }
            Ctx.SaveChanges();
        }

        public void Remove(int gameId, int? botId, int chrominoId)
        {
            List<ComputedChromino> ccToRemove = (from cc in Ctx.ComputedChrominos
                                                 where cc.GameId == gameId && cc.PlayerId == botId && cc.ChrominoId == chrominoId
                                                 select cc).ToList();

            if (ccToRemove.Count > 0)
            {
                Ctx.ComputedChrominos.RemoveRange(ccToRemove);
                Ctx.SaveChanges();
            }
        }

        public void Remove(int gameId, int botId)
        {
            List<ComputedChromino> ccToRemove = (from cc in Ctx.ComputedChrominos
                                                 where cc.GameId == gameId && cc.PlayerId == botId
                                                 select cc).ToList();

            if (ccToRemove != null)
                Ctx.ComputedChrominos.RemoveRange(ccToRemove);

            Ctx.SaveChanges();
        }

        public void Add(List<ComputedChromino> chrominosFound)
        {
            foreach (var toAdd in chrominosFound)
                Ctx.ComputedChrominos.Add(toAdd);

            Ctx.SaveChanges();
        }

        /// <summary>
        /// renvoie la liste des chrominos pouvant être joué au tour actuel classé par priorité
        /// </summary>
        /// <param name="gameId">Id de la partie</param>
        /// <param name="botId">Id du bot joueur</param>
        /// <param name="chominoIdNotToPlay">Id du chromino ne devant pas être joué</param>
        /// <returns></returns>
        public List<ComputedChromino> RootListByPriority(int gameId, int botId, int chominoIdNotToPlay)
        {
            List<ComputedChromino> ComputedChrominos = (from cc in Ctx.ComputedChrominos
                                                        join c in Ctx.Chrominos on cc.ChrominoId equals c.Id
                                                        where cc.GameId == gameId && cc.PlayerId == botId && cc.ParentId == null && cc.ChrominoId != chominoIdNotToPlay
                                                        orderby c.Points, c.SecondColor == ColorCh.Cameleon, c.Id
                                                        select cc).AsNoTracking().ToList();

            return ComputedChrominos;
        }
    }
}
