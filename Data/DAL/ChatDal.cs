using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class ChatDal
    {
        private readonly Context Ctx;

        public ChatDal(Context context)
        {
            Ctx = context;
        }

        public Chat Details(int id)
        {
            return (from c in Ctx.Chats
                    where c.Id == id
                    select c).FirstOrDefault();
        }

        public void Add(int gamePlayerId, string message)
        {
            Chat chat = new Chat { GamePlayerId = gamePlayerId, Date = DateTime.Now, Message = message };
            Ctx.Chats.Add(chat);
            Ctx.SaveChanges();
        }

        public List<Chat> GetMessages(int gameId, DateTime dateMin, DateTime dateMax)
        {
            return (from c in Ctx.Chats
                    join gp in Ctx.GamesPlayers on c.GamePlayerId equals gp.Id
                    where gp.GameId == gameId && c.Date > dateMin && c.Date <= dateMax
                    select c).ToList();
        }

        public int NewMessagesNumber(int gameId, DateTime dateMin)
        {
            return (from c in Ctx.Chats
                    join gp in Ctx.GamesPlayers on c.GamePlayerId equals gp.Id
                    where gp.GameId == gameId && c.Date > dateMin
                    select c).Count();
        }
    }
}
