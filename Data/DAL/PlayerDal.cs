using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Data.DAL
{
    public class PlayerDal
    {

        private readonly DefaultContext Ctx;

        public PlayerDal(DefaultContext context)
        {
            Ctx = context;
        }

        public List<Player> List()
        {

            var result = (from players in Ctx.Players
                          select players).ToList();

            return result;
        }

        public int BotId()
        {

            Player botplayer = (from p in Ctx.Players
                                where p.Pseudo == "Bot"
                                select p).FirstOrDefault();

            if (botplayer == null)
                botplayer = CreateBotPlayer();

            return botplayer.Id;
        }

        public Player Bot()
        {

            Player botplayer = (from p in Ctx.Players
                                where p.Pseudo == "Bot"
                                select p).FirstOrDefault();

            if (botplayer == null)
                botplayer = CreateBotPlayer();

            return botplayer;
        }

        public Player Detail(int id)
        {

            Player player = (from p in Ctx.Players
                             where p.Id == id
                             select p).FirstOrDefault();

            return player;
        }

        private Player CreateBotPlayer()
        {
            Player player = new Player { Pseudo = "Bot", Password = "password" };
            Ctx.Add(player);
            Ctx.SaveChanges();
            return player;
        }
    }
}
