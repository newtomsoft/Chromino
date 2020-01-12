using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data.Enumeration;

namespace Data.DAL
{
    public class GameDal
    {
        private readonly DefaultContext Ctx;

        public GameDal(DefaultContext context)
        {
            Ctx = context;
        }

        public Game Details(string guid)
        {

            Game game = (from games in Ctx.Games
                         where games.Guid == guid
                         select games).FirstOrDefault();

            return game;
        }

        public Game Details(int id)
        {

            Game game = (from games in Ctx.Games
                         where games.Id == id
                         select games).FirstOrDefault();

            return game;
        }

        public List<Game> List()
        {

            var games = (from g in Ctx.Games
                         select g).ToList();

            return games;
        }

        public Game AddGame()
        {
            string guid = Guid.NewGuid().ToString("N");
            Game game = new Game { CreateDate = DateTime.Now, Guid = guid, AutoPlay = false };
            Ctx.Games.Add(game);
            Ctx.SaveChanges();
            return Details(guid);
        }

        public void SetStatus(int id, GameStatus status)
        {
            Game game = (from g in Ctx.Games
                         where g.Id == id
                         select g).FirstOrDefault();

            game.Status = status;
            Ctx.SaveChanges();
        }

        public void SetAutoPlay(int id, bool autoPlay)
        {
            Game game = (from g in Ctx.Games
                         where g.Id == id
                         select g).FirstOrDefault();

            game.AutoPlay = autoPlay;
            Ctx.SaveChanges();
        }
    }
}
