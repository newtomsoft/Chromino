using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Data.Core;

namespace Data.DAL
{
    public class GamePlayerDal
    {
        private readonly DefaultContext Ctx;
        public GamePlayerDal(DefaultContext context)
        {
            Ctx = context;
        }

        public GamePlayer Details(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            return gamePlayer;
        }

        public void Add(int gameId, List<Player> players)
        {
            List<GamePlayer> gamePlayers = new List<GamePlayer>();
            foreach (Player player in players)
            {
                GamePlayer gamePlayer = new GamePlayer
                {
                    GameId = gameId,
                    PlayerId = player.Id,
                };
                gamePlayers.Add(gamePlayer);
            }
            Ctx.GamesPlayers.AddRange(gamePlayers);
            Ctx.SaveChanges();
        }

        public List<Player> Players(int gameId)
        {
            List<Player> players = (from gp in Ctx.GamesPlayers
                                    join p in Ctx.Players on gp.PlayerId equals p.Id
                                    where gp.GameId == gameId
                                    select p).ToList();

            return players;
        }

        public int PlayersNumber(int gameId)
        {
            int playersNumber = (from gp in Ctx.GamesPlayers
                                 join p in Ctx.Players on gp.PlayerId equals p.Id
                                 where gp.GameId == gameId
                                 select p).Count();

            return playersNumber;
        }

        public List<int> PlayersId(int gameId)
        {
            List<int> playersId = (from gp in Ctx.GamesPlayers
                                   join p in Ctx.Players on gp.PlayerId equals p.Id
                                   where gp.GameId == gameId
                                   select p.Id).ToList();

            return playersId;
        }

        public void AddPoints(int gameId, int playerId, int points)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.Points += points;
            Ctx.SaveChanges();
        }

        public List<Game> GamesInProgress(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && g.Status == GameStatus.InProgress
                                orderby g.PlayedDate
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> GamesWon(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && g.Status == GameStatus.Finished && gp.Win
                                orderby g.PlayedDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> GamesLost(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && g.Status == GameStatus.Finished && !gp.Win
                                orderby g.PlayedDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> MultiGamesToPlay(int playerId)
        {
            var multiGamesId = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where g.Status == GameStatus.InProgress
                                orderby g.PlayedDate
                                group gp by gp.GameId into groupe
                                where groupe.Count() > 1
                                select groupe.Key).ToList();

            List<Game> multiGames = new List<Game>();
            foreach (int id in multiGamesId)
            {
                multiGames.Add(Ctx.Games.Find(id));
            }

            var singleGameInProgressId = (from gp in Ctx.GamesPlayers
                                          join g in Ctx.Games on gp.GameId equals g.Id
                                          where g.Status == GameStatus.InProgress
                                          orderby g.PlayedDate
                                          group gp by gp.GameId into groupe
                                          where groupe.Count() == 1
                                          select groupe.Key).ToList();

            List<Game> singleGameInProgress = new List<Game>();
            foreach (int id in singleGameInProgressId)
            {
                singleGameInProgress.Add(Ctx.Games.Find(id));
            }

            var gamesToPlay = (from gp in Ctx.GamesPlayers
                               join g in Ctx.Games on gp.GameId equals g.Id
                               where gp.PlayerId == playerId && gp.Turn && g.Status == GameStatus.InProgress
                               orderby g.PlayedDate
                               select g).AsNoTracking().ToList();

            List<Game> multiGameToPlay = gamesToPlay.Intersect(multiGames, new GameEqualityComparer()).ToList();

            return multiGameToPlay;
        }

        public IEnumerable<Game> SingleGamesFinished(int playerId)
        {
            var allSingleGameFinishedId = (from gp in Ctx.GamesPlayers
                                           join g in Ctx.Games on gp.GameId equals g.Id
                                           where g.Status == GameStatus.SingleFinished
                                           orderby g.PlayedDate descending
                                           group gp by gp.GameId into groupe
                                           where groupe.Count() == 1
                                           select groupe.Key).ToList();

            List<Game> allSingleGameFinished = new List<Game>();
            foreach (int id in allSingleGameFinishedId)
            {
                allSingleGameFinished.Add(Ctx.Games.Find(id));
            }

            var gamesFinished = (from gp in Ctx.GamesPlayers
                                 join g in Ctx.Games on gp.GameId equals g.Id
                                 where gp.PlayerId == playerId && gp.Turn && g.Status == GameStatus.SingleFinished
                                 orderby g.PlayedDate descending
                                 select g).AsNoTracking().ToList();

            List<Game> singleGameFinished = gamesFinished.Intersect(allSingleGameFinished, new GameEqualityComparer()).ToList();

            return singleGameFinished;
        }

        public List<Game> SingleGamesInProgress(int playerId)
        {
            var allSingleGameInProgressId = (from gp in Ctx.GamesPlayers
                                             join g in Ctx.Games on gp.GameId equals g.Id
                                             where g.Status == GameStatus.InProgress
                                             orderby g.PlayedDate
                                             group gp by gp.GameId into groupe
                                             where groupe.Count() == 1
                                             select groupe.Key).ToList();

            List<Game> allSingleGameInProgress = new List<Game>();
            foreach (int id in allSingleGameInProgressId)
            {
                allSingleGameInProgress.Add(Ctx.Games.Find(id));
            }

            var gamesToPlay = (from gp in Ctx.GamesPlayers
                               join g in Ctx.Games on gp.GameId equals g.Id
                               where gp.PlayerId == playerId && gp.Turn && g.Status == GameStatus.InProgress
                               orderby g.PlayedDate
                               select g).AsNoTracking().ToList();

            List<Game> singleGameInProgress = gamesToPlay.Intersect(allSingleGameInProgress, new GameEqualityComparer()).ToList();

            return singleGameInProgress;
        }

        public List<Game> Games(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId
                                orderby g.CreateDate
                                select g).AsNoTracking().ToList();

            return games;
        }

        public Player PlayerTurn(int gameId)
        {
            Player player = (from gp in Ctx.GamesPlayers
                             join p in Ctx.Players on gp.PlayerId equals p.Id
                             where gp.GameId == gameId && gp.Turn
                             select p).FirstOrDefault();

            return player;
        }

        public void SetPreviouslyDraw(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.PreviouslyDraw = true;
            Ctx.SaveChanges();
        }

        public bool IsAllPass(int gameId)
        {
            var passs = from gp in Ctx.GamesPlayers
                        where gp.GameId == gameId
                        select gp.PreviouslyPass;

            if (passs.Contains(false))
                return false;
            else
                return true;
        }

        public void SetPass(int gameId, int playerId, bool pass)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.PreviouslyPass = pass;
            Ctx.SaveChanges();
        }

        public void SetWon(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.Win = true;
            Ctx.SaveChanges();
        }
    }
}
