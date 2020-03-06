using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class GameDal
    {
        private readonly Context Ctx;

        public GameDal(Context context)
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


        public List<Game> ListInProgress()
        {

            var games = (from g in Ctx.Games
                         where g.Status == GameStatus.InProgress || g.Status == GameStatus.SingleInProgress
                         select g).ToList();

            return games;
        }

        public Game Add()
        {
            string guid = Guid.NewGuid().ToString("N");
            Game game = new Game { CreateDate = DateTime.Now, Guid = guid };
            Ctx.Games.Add(game);
            Ctx.SaveChanges();
            return Details(guid);
        }

        public void SetStatus(int gameId, GameStatus status)
        {
            Game game = (from g in Ctx.Games
                         where g.Id == gameId
                         select g).FirstOrDefault();

            game.Status = status;
            Ctx.SaveChanges();
        }

        public GameStatus GetStatus(int gameId)
        {
            GameStatus status = (from g in Ctx.Games
                                 where g.Id == gameId
                                 select g.Status).FirstOrDefault();

            return status;
        }

        public bool IsFinished(int gameId)
        {
            int id = (from g in Ctx.Games
                      where g.Id == gameId && (g.Status == GameStatus.Finished || g.Status == GameStatus.SingleFinished)
                      select g.Id).FirstOrDefault();

            return id == 0 ? false : true;
        }

        public void UpdateDate(int id)
        {
            Game game = (from g in Ctx.Games
                         where g.Id == id
                         select g).FirstOrDefault();

            game.PlayedDate = DateTime.Now;
            Ctx.SaveChanges();
        }
    }
}
