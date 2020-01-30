using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        public Player Details(int id)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == id
                             select p).FirstOrDefault();

            return player;
        }

        public Player Details(string pseudo)
        {
            Player player = (from p in Ctx.Players
                             where p.Pseudo == pseudo
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

        public Player GetPlayer(Player player)
        {
            var result = (from p in Ctx.Players
                          where p.Pseudo == player.Pseudo && p.Password == player.Password
                          select p).AsNoTracking().FirstOrDefault();

            return result;
        }

        public void AddPoints(int playerId, int points)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            player.Points += points;
            Ctx.SaveChanges();
        }

        public void AddSinglePlayerGamesPoints(int playerId, int points)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            player.PointsSinglePlayerGames += points;
            Ctx.SaveChanges();
        }
    }
}
