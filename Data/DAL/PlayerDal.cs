using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class PlayerDal
    {
        private readonly Context Ctx;

        public PlayerDal(Context context)
        {
            Ctx = context;
        }

        public List<Player> List()
        {
            var result = (from players in Ctx.Players
                          select players).ToList();

            return result;
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

        public bool IsBot(int playerId)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            return player.Bot;
        }

        public void IncreaseWin(int playerId)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            player.WonGames++;
            Ctx.SaveChanges();
        }

        public List<int> BotsId()
        {
            List<int> botsId = (from p in Ctx.Players
                                where p.Bot
                                select p.Id).ToList();

            return botsId;
        }
    }
}
