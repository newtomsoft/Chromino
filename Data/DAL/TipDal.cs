using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        public Tip Details(TipName tipName)
        {
            var result = (from t in Ctx.Tips
                          where t.TipName == tipName
                          select t).AsNoTracking().FirstOrDefault();

            return result;
        }

        public List<TipName> ListNamesOn(int playerId)
        {
            var tipsoff = from t in Ctx.Tips
                          join toff in Ctx.TipsOff on t.Id equals toff.TipId
                          where toff.PlayerId == playerId
                          select t.TipName;

            return ListNames().Except(tipsoff).ToList();
        }

        public List<Tip> ListOn(int playerId)
        {
            var tipsoff = from t in Ctx.Tips
                          join toff in Ctx.TipsOff on t.Id equals toff.TipId
                          where toff.PlayerId == playerId
                          select t;

            return List().Except(tipsoff).ToList();
        }

        public void SetOff(int playerId, int tipid)
        {
            TipOff tipOff = new TipOff { TipId = tipid, PlayerId = playerId };
            Ctx.TipsOff.Add(tipOff);
            Ctx.SaveChanges();
        }

        private IQueryable<TipName> ListNames()
        {
            var results = from t in Ctx.Tips
                          select t.TipName;

            return results;
        }

        private IQueryable<Tip> List()
        {
            var results = from t in Ctx.Tips
                          select t;

            return results;
        }
    }
}
