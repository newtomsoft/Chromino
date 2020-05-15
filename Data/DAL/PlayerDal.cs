using Data.Models;
using System;
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
            return (from players in Ctx.Players
                    select players).ToList();
        }

        public object ListIdsNames()
        {
            return (from p in Ctx.Players
                    join ur in Ctx.UserRoles on p.Id equals ur.UserId
                    join r in Ctx.Roles on ur.RoleId equals r.Id
                    where !p.Bot && r.Name != "Guest" && r.Name != "Bot"
                    select new { id = p.Id, name = p.UserName }).ToList();
        }

        public IQueryable<Player> ListGuestWithOldGames(TimeSpan lifeTime, out IQueryable<int> gamesId)
        {
            DateTime ToCompare = DateTime.Now - lifeTime;
            var guestsAndGames = from g in Ctx.Games
                                 join gp in Ctx.GamesPlayers on g.Id equals gp.GameId
                                 join p in Ctx.Players on gp.PlayerId equals p.Id
                                 where p.UserName.StartsWith("invit") && g.PlayedDate < ToCompare
                                 select new { guests = p, gamesId = g.Id };

            var allGuests = from p in Ctx.Players
                            where p.UserName.StartsWith("invit")
                            select p;

            var guestsWithGame = from p in Ctx.Players
                                 join gp in Ctx.GamesPlayers on p.Id equals gp.PlayerId
                                 join g in Ctx.Games on gp.GameId equals g.Id
                                 where p.UserName.StartsWith("invit")
                                 select p;

            var addguests = allGuests.Except(guestsWithGame);

            var guests = (from gg in guestsAndGames
                          select gg.guests).Distinct();

            guests = guests.Concat(addguests);

            gamesId = from gg in guestsAndGames
                      select gg.gamesId;

            return guests;
        }

        public Player Details(int id)
        {
            return (from p in Ctx.Players
                    where p.Id == id
                    select p).FirstOrDefault();
        }

        public Player Details(string pseudo)
        {
            return (from p in Ctx.Players
                    where p.UserName == pseudo
                    select p).FirstOrDefault();
        }
        public string Name(int id)
        {
            return (from p in Ctx.Players
                    where p.Id == id
                    select p.UserName).FirstOrDefault();
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

        public void IncreaseWinAndHelp(int playerId, int helpIncrease)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            player.WonGames++;
            player.Help += helpIncrease;
            Ctx.SaveChanges();
        }

        public void IncreaseHelp(int playerId, int increaseHelp)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            player.Help += increaseHelp;
            Ctx.SaveChanges();
        }

        public List<int> BotsId()
        {
            return (from p in Ctx.Players
                    where p.Bot
                    select p.Id).ToList();
        }

        public bool DecreaseHelp(int playerId)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            if (player.Help > 0)
            {
                player.Help--;
                Ctx.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IncreaseHelp(int playerId)
        {
            Player player = (from p in Ctx.Players
                             where p.Id == playerId
                             select p).FirstOrDefault();

            if (player.Help < Player.MaxHelp)
            {
                player.Help++;
                Ctx.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Delete(IQueryable<Player> players)
        {
            Ctx.Players.RemoveRange(players);
            return Ctx.SaveChanges();
        }

        public int Delete(int playerId)
        {
            Ctx.Players.Remove(Details(playerId));
            return Ctx.SaveChanges();
        }
    }
}

