using Data.Enumeration;
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

        public List<GamePlayer> List(int gameId)
        {
            List<GamePlayer> gamesPlayer = (from gp in Ctx.GamesPlayers
                                            where gp.GameId == gameId
                                            select gp).ToList();

            return gamesPlayer;
        }

        public int PlayerId(int gameId, string playerPseudo)
        {
            int playerId = (from gp in Ctx.GamesPlayers
                            join p in Ctx.Players on gp.PlayerId equals p.Id
                            where gp.GameId == gameId && p.UserName == playerPseudo
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

        public List<Game> GamesAgainstBotsOnly(int playerId)
        {
            // groupement de tous les GamePlayer (sans le joueur concerné) par Id de jeu Humain étant le nombre d'adversaires humains
            var resultQuery1 = from gp in Ctx.GamesPlayers
                               join p in Ctx.Players on gp.PlayerId equals p.Id
                               where p.Id != playerId
                               select new { gp.GameId, p.Bot } into idnBot
                               group idnBot by idnBot.GameId into grp
                               select new { Id = grp.Key, Humain = grp.Sum(x => x.Bot ? 0 : 1) };

            // Obtention des Id des jeux qui n'ont pas d'humain en adversaire du joueur
            var idGamesNoHuman = from g in resultQuery1
                                 where g.Humain == 0
                                 select g.Id;

            var games = (from gp in Ctx.GamesPlayers
                         join gnh in idGamesNoHuman on gp.GameId equals gnh
                         join g in Ctx.Games on gp.GameId equals g.Id
                         where gp.PlayerId == playerId && !gp.ViewFinished && g.Status != GameStatus.SingleFinished && g.Status != GameStatus.SingleInProgress
                         orderby g.PlayedDate
                         select g).ToList();

            return games;
        }

        public List<Game> MultiGamesAgainstAtLeast1HumanToPlay(int playerId)
        {
            var firstFilter = (from gp in Ctx.GamesPlayers
                               join g in Ctx.Games on gp.GameId equals g.Id
                               join p in Ctx.Players on gp.PlayerId equals p.Id
                               where !p.Bot && p.Id != playerId
                               select g).AsNoTracking();

            var games = (from gp in Ctx.GamesPlayers
                         join g in firstFilter on gp.GameId equals g.Id
                         where gp.PlayerId == playerId && gp.Turn && !gp.ViewFinished && g.Status != GameStatus.SingleFinished && g.Status != GameStatus.SingleInProgress
                         orderby g.PlayedDate
                         select g).AsNoTracking().ToList();

            return games;
        }

        public int FirstIdMultiGameToPlay(int playerId)
        {
            Game game = MultiGamesAgainstAtLeast1HumanToPlay(playerId).FirstOrDefault();
            return game != null ? game.Id : 0;
        }

        public List<Game> SingleGamesFinished(int playerId)
        {
            List<Game> singleGames = (from gp in Ctx.GamesPlayers
                                      join g in Ctx.Games on gp.GameId equals g.Id
                                      where gp.PlayerId == playerId && g.Status == GameStatus.SingleFinished
                                      orderby g.PlayedDate descending
                                      select g).AsNoTracking().ToList();

            return singleGames;
        }

        public List<Game> SingleGamesInProgress(int playerId)
        {
            List<Game> singleGames = (from gp in Ctx.GamesPlayers
                                      join g in Ctx.Games on gp.GameId equals g.Id
                                      where gp.PlayerId == playerId && g.Status == GameStatus.SingleInProgress
                                      orderby g.PlayedDate
                                      select g).AsNoTracking().ToList();

            return singleGames;
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
        /// enregistre si le joueur a pasé son tour ou non
        /// si je joueur passe, augmente le nombre de mouvement (Move) du jeu
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

            if (pass)
            {
                Game game = (from g in Ctx.Games
                             where g.Id == gameId
                             select g).FirstOrDefault();

                game.Move++;
            }
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

        /// <summary>
        /// retourne l'Id du dernier joueur du tour du jeu
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        public int RoundLastPlayerId(int gameId)
        {
            int playerId = (from gp in Ctx.GamesPlayers
                            where gp.GameId == gameId
                            orderby gp.Id descending
                            select gp.PlayerId).FirstOrDefault();

            return playerId;
        }

        /// <summary>
        /// retourne la liste des joueurs à jouer après le joueur renseigné
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <param name="playerId">Id du joueur à renseigner</param>
        /// <returns></returns>
        public List<int> NextPlayersId(int gameId, int playerId)
        {
            int gamePlayerId = Details(gameId, playerId).Id;

            List<int> playersId = (from gp in Ctx.GamesPlayers
                                   where gp.GameId == gameId && gp.Id > gamePlayerId
                                   select gp.PlayerId).ToList();

            return playersId;
        }

        public bool IsSomePlayerWon(int gameId)
        {
            int somePlayerWon = (from gp in Ctx.GamesPlayers
                                 where gp.GameId == gameId && gp.Win == true
                                 select gp.PlayerId).FirstOrDefault();

            return somePlayerWon == 0 ? false : true;
        }

        public void ChangeMemo(int gameId, int playerId, string memo)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.Memo = memo;
            Ctx.SaveChanges();
        }
    }
}
