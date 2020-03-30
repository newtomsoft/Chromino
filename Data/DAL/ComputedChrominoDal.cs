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

        public void Remove(int gameId, int botId, HashSet<ComputedChromino> computedChrominosToRemove)
        {
            foreach (var toRemove in computedChrominosToRemove)
            {
                ComputedChromino ccToRemove = (from cc in Ctx.ComputedChrominos
                                               where cc.GameId == gameId && cc.BotId == botId && cc.X == toRemove.X && cc.Y == toRemove.Y && cc.Orientation == toRemove.Orientation
                                               select cc).FirstOrDefault();

                if (ccToRemove != null)
                    Ctx.ComputedChrominos.Remove(ccToRemove);

                switch (toRemove.Orientation)
                {
                    case Orientation.Horizontal:
                        ccToRemove = (from cc in Ctx.ComputedChrominos
                                      where cc.GameId == gameId && cc.BotId == botId && cc.X == toRemove.X + 2 && cc.Y == toRemove.Y && cc.Orientation == Orientation.HorizontalFlip
                                      select cc).FirstOrDefault();
                        break;
                    case Orientation.HorizontalFlip:
                        ccToRemove = (from cc in Ctx.ComputedChrominos
                                      where cc.GameId == gameId && cc.BotId == botId && cc.X == toRemove.X - 2 && cc.Y == toRemove.Y && cc.Orientation == Orientation.Horizontal
                                      select cc).FirstOrDefault();
                        break;
                    case Orientation.Vertical:
                        ccToRemove = (from cc in Ctx.ComputedChrominos
                                      where cc.GameId == gameId && cc.BotId == botId && cc.X == toRemove.X && cc.Y == toRemove.Y - 2 && cc.Orientation == Orientation.VerticalFlip
                                      select cc).FirstOrDefault();
                        break;
                    case Orientation.VerticalFlip:
                        ccToRemove = (from cc in Ctx.ComputedChrominos
                                      where cc.GameId == gameId && cc.BotId == botId && cc.X == toRemove.X && cc.Y == toRemove.Y + 2 && cc.Orientation == Orientation.Vertical
                                      select cc).FirstOrDefault();
                        break;

                }
                if (ccToRemove != null)
                    Ctx.ComputedChrominos.Remove(ccToRemove);
            }
            Ctx.SaveChanges();
        }

        public void Remove(int gameId, int botId, int chrominoId)
        {
            List<ComputedChromino> ccToRemove = (from cc in Ctx.ComputedChrominos
                                                 where cc.GameId == gameId && cc.BotId == botId && cc.ChrominoId == chrominoId
                                                 select cc).ToList();

            if (ccToRemove != null)
                Ctx.ComputedChrominos.RemoveRange(ccToRemove);

            Ctx.SaveChanges();
        }

        public void Remove(int gameId, int botId)
        {
            List<ComputedChromino> ccToRemove = (from cc in Ctx.ComputedChrominos
                                                 where cc.GameId == gameId && cc.BotId == botId
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
                                                        where cc.GameId == gameId && cc.BotId == botId && cc.ParentId == null && cc.ChrominoId != chominoIdNotToPlay
                                                        orderby c.Points, c.SecondColor == ColorCh.Cameleon, c.Id
                                                        select cc).AsNoTracking().ToList();

            return ComputedChrominos;
        }
    }
}
