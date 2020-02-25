﻿using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class GamePlayerDal
    {
        private readonly Context Ctx;
        public GamePlayerDal(Context context)
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

        public int PlayerId(int gameId, string playerPseudo)
        {
            int playerId = (from gp in Ctx.GamesPlayers
                            join p in Ctx.Players on gp.PlayerId equals p.Id
                            where gp.GameId == gameId && p.Pseudo == playerPseudo
                            select p.Id).FirstOrDefault();

            return playerId;
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
                                 where gp.GameId == gameId
                                 select gp.Id).Count();

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

        public List<Game> GamesWaitTurn(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && !gp.ViewFinished && !gp.Turn
                                orderby g.PlayedDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> GamesWon(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && gp.ViewFinished && gp.Win == true
                                orderby g.PlayedDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> GamesLost(int playerId)
        {
            List<Game> games = (from gp in Ctx.GamesPlayers
                                join g in Ctx.Games on gp.GameId equals g.Id
                                where gp.PlayerId == playerId && gp.ViewFinished && gp.Win == false
                                orderby g.PlayedDate descending
                                select g).AsNoTracking().ToList();

            return games;
        }

        public List<Game> MultiGamesToPlay(int playerId)
        {
            List<Game> multiGameToPlay = (from gp in Ctx.GamesPlayers
                                          join g in Ctx.Games on gp.GameId equals g.Id
                                          where gp.PlayerId == playerId && gp.Turn && !gp.ViewFinished && g.Status != GameStatus.SingleFinished && g.Status != GameStatus.SingleInProgress
                                          orderby g.PlayedDate
                                          select g).AsNoTracking().ToList();

            return multiGameToPlay;
        }

        public IEnumerable<Game> SingleGamesFinished(int playerId)
        {
            List<Game> singleGameFinished = (from gp in Ctx.GamesPlayers
                                             join g in Ctx.Games on gp.GameId equals g.Id
                                             where gp.PlayerId == playerId && g.Status == GameStatus.SingleFinished
                                             orderby g.PlayedDate descending
                                             select g).AsNoTracking().ToList();

            return singleGameFinished;
        }

        public List<Game> SingleGamesInProgress(int playerId)
        {
            List<Game> singleGameFinished = (from gp in Ctx.GamesPlayers
                                             join g in Ctx.Games on gp.GameId equals g.Id
                                             where gp.PlayerId == playerId && g.Status == GameStatus.SingleInProgress
                                             orderby g.PlayedDate
                                             select g).AsNoTracking().ToList();

            return singleGameFinished;
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

        public bool IsPreviouslyDraw(int gameId, int playerId)
        {
            bool previouslyDraw = (from gp in Ctx.GamesPlayers
                                   where gp.GameId == gameId && gp.PlayerId == playerId
                                   select gp.PreviouslyDraw).FirstOrDefault();

            return previouslyDraw;
        }

        public bool GetViewFinished(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            return gamePlayer.ViewFinished;
        }

        public void SetViewFinished(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.ViewFinished = true;
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

        /// <summary>
        /// indique si le joueur a pasé son tour
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <param name="playerId">id du joueur</param>
        /// <param name="pass">true : le joueur a passé son tour</param>
        public void SetPass(int gameId, int playerId, bool pass)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.PreviouslyPass = pass;
            Ctx.SaveChanges();
        }

        public bool? GetWon(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            return gamePlayer.Win;
        }

        public void SetWon(int gameId, int playerId, bool won = true)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.Win = won;
            Ctx.SaveChanges();
        }

        public bool IsAllBot(int gameId)
        {
            var ids = from gp in Ctx.GamesPlayers
                      join p in Ctx.Players on gp.PlayerId equals p.Id
                      where gp.GameId == gameId
                      select p.Bot;

            if (ids.Contains(false))
                return false;
            else
                return true;
        }

        public int RoundLastPlayerId(int gameId)
        {
            int playerId = (from gp in Ctx.GamesPlayers
                            where gp.GameId == gameId
                            orderby gp.Id descending
                            select gp.PlayerId).FirstOrDefault();

            return playerId;
        }

        public bool IsSomePlayerWon(int gameId)
        {
            int somePlayerWon = (from gp in Ctx.GamesPlayers
                                 where gp.GameId == gameId && gp.Win == true
                                 select gp.PlayerId).FirstOrDefault();

            return somePlayerWon == 0 ? false : true;
        }
    }
}
