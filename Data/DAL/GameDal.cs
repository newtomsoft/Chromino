﻿using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public partial class GameDal
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

        /// <summary>
        /// nombre de coups joué dans la partie
        /// le premier chromino placé aléatoirement n'est pas compté
        /// le coup en cours (non joué) n'est pas compté
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte Moves(int id)
        {
            byte move = (from games in Ctx.Games
                         where games.Id == id
                         select games.Move).FirstOrDefault();

            return (byte)(move - 1);
        }

        public List<Game> List()
        {

            var games = (from g in Ctx.Games
                         select g).ToList();

            return games;
        }

        public List<string> ListGuids(int playerId)
        {
            var guids = (from g in Ctx.Games
                         join gp in Ctx.GamesPlayers on g.Id equals gp.GameId
                         join p in Ctx.Players on gp.PlayerId equals p.Id
                         where p.Id == playerId
                         select g.Guid).ToList();

            return guids;
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
            Game game = new Game { CreateDate = DateTime.Now, Guid = guid, Move = 0 };
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
            return GetStatus(gameId).IsFinished();
        }

        public void UpdateDate(int id)
        {
            Game game = (from g in Ctx.Games
                         where g.Id == id
                         select g).FirstOrDefault();

            game.PlayedDate = DateTime.Now;
            Ctx.SaveChanges();
        }

        public void IncreaseMove(int gameId)
        {
            Game game = (from games in Ctx.Games
                         where games.Id == gameId
                         select games).FirstOrDefault();

            game.Move++;
            Ctx.SaveChanges();
        }

        public int Delete(IQueryable<int> idToDelete)
        {
            var result = from g in Ctx.Games
                         where idToDelete.Contains(g.Id)
                         select g;

            Ctx.Games.RemoveRange(result);
            return Ctx.SaveChanges();
        }

        public bool IsExist(int gameId)
        {
            return Details(gameId) != null ? true : false;
        }
    }
}
