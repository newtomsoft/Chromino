using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class TipDal
    {
        private readonly Context Ctx;

        public TipDal(Context context)
        {
            Ctx = context;
        }

        public Tip Details(int id)
        {
            var result = (from t in Ctx.Tips
                          where t.Id == id
                          select t).AsNoTracking().FirstOrDefault();

            return result;
        }

        public TipOff Details(int playerId, int tipId)
        {
            var result = (from t in Ctx.TipsOff
                          where t.TipId == tipId && t.PlayerId == playerId
                          select t).AsNoTracking().FirstOrDefault();

            return result;
        }

        public List<string> ListNamesOn(int playerId)
        {
            var tipsoff = from t in Ctx.Tips
                          join toff in Ctx.TipsOff on t.Id equals toff.TipId
                          where toff.PlayerId == playerId
                          select t.Name;

            return Names().Except(tipsoff).ToList();
        }

        public List<Tip> ListOn(int playerId)
        {
            var tipsoff = from t in Ctx.Tips
                          join toff in Ctx.TipsOff on t.Id equals toff.TipId
                          where toff.PlayerId == playerId
                          select t;

            return Ctx.Tips.Except(tipsoff).ToList();
        }

        public void SetOff(int playerId, int tipid)
        {
            TipOff tipOff = new TipOff { TipId = tipid, PlayerId = playerId };
            Ctx.TipsOff.Add(tipOff);
            Ctx.SaveChanges();
        }

        public void SetAllOff(int playerId)
        {
            List<Tip> tips = Ctx.Tips.AsNoTracking().ToList();
            foreach (Tip tip in tips)
            {
                if (Details(playerId, tip.Id) == null)
                {
                    TipOff tipOff = new TipOff { TipId = tip.Id, PlayerId = playerId };
                    Ctx.TipsOff.Add(tipOff);
                }
            }
            Ctx.SaveChanges();
        }

        private IQueryable<string> Names()
        {
            var results = from t in Ctx.Tips
                          select t.Name;

            return results;
        }
    }
}
