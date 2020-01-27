using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public GamePlayer GamePlayer(int gameId, int playerId)
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
        public List<int> PlayersId(int gameId)
        {
            List<int> playersId = (from gp in Ctx.GamesPlayers
                                   join p in Ctx.Players on gp.PlayerId equals p.Id
                                   where gp.GameId == gameId
                                   select p.Id).ToList();

            return playersId;
        }

        public void AddPoint(int playerId, int point)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.PlayerPoints += point;
            Ctx.SaveChanges();
        }
    }
}
