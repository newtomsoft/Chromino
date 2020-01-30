using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Data.DAL
{
    public class GamePlayerDal
    {
        private readonly DefaultContext Ctx;
        public GamePlayerDal(DefaultContext context)
        {
            Ctx = context;
        }

        public GamePlayer GetBot(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            if (gamePlayer == null)
            {
                gamePlayer = new GamePlayer { GameId = gameId, PlayerId = playerId };
                Ctx.Add(gamePlayer);
                Ctx.SaveChanges();
            }

            return gamePlayer;
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

        public void AddPoint(int gameId, int playerId, int points)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.PlayerPoints += points;
            Ctx.SaveChanges();
        }

        public List<Game> GamesInProgress(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && g.Status == GameStatus.InProgress
                                orderby g.CreateDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> GamesFinished(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && g.Status == GameStatus.Finished
                                orderby g.CreateDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> GamesToPlay(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && gp.PlayerTurn
                                orderby g.CreateDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> Games(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId
                                orderby g.CreateDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public Player PlayerTurn(int gameId)
        {
            Player player = (from gp in Ctx.GamesPlayers
                             join p in Ctx.Players on gp.PlayerId equals p.Id
                             where gp.GameId == gameId && gp.PlayerTurn
                             select p).FirstOrDefault();

            return player;
        }

        public int ChrominosNumber(int gameId, int playerId)
        {
            int ChrominosNumber = (from gp in Ctx.GamesPlayers
                                   join gc in Ctx.GamesChrominos on gp.GameId equals gc.GameId
                                   where gp.GameId == gameId && gp.PlayerId == playerId && gc.State == ChrominoStatus.InPlayer
                                   select gc).Count();

            return ChrominosNumber;
        }
    }
}
