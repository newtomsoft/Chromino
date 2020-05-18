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
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.PlayerId == playerId
                    select gp).FirstOrDefault();
        }

        public int Id(int gameId, int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.PlayerId == playerId
                    select gp.Id).FirstOrDefault();
        }

        public bool IsPlayerIdIn(int gameId, int playerId)
        {
            return Details(gameId, playerId) != null ? true : false;
        }

        public List<GamePlayer> GamePlayers(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId
                    select gp).ToList();
        }

        public List<int> PlayersId(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId
                    select gp.PlayerId).ToList();

        }

        public Dictionary<int, string> PlayersIdName(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    join p in Ctx.Players on gp.PlayerId equals p.Id
                    where gp.GameId == gameId
                    select new { id = p.Id, name = p.UserName }).ToDictionary(x => x.id, x => x.name);
        }

        public Dictionary<int, string> GamePlayersIdPlayerName(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    join p in Ctx.Players on gp.PlayerId equals p.Id
                    where gp.GameId == gameId
                    select new { id = gp.Id, name = p.UserName }).ToDictionary(x => x.id, x => x.name);
        }

        public List<int> WinnersId(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.Win == true
                    select gp.PlayerId).ToList();
        }

        public int PlayerId(int gameId, string playerPseudo)
        {
            return (from gp in Ctx.GamesPlayers
                    join p in Ctx.Players on gp.PlayerId equals p.Id
                    where gp.GameId == gameId && p.UserName == playerPseudo
                    select p.Id).FirstOrDefault();
        }

        public void Add(int gameId, List<Player> players)
        {
            List<GamePlayer> gamePlayers = new List<GamePlayer>();
            foreach (Player player in players)
            {
                GamePlayer gamePlayer = new GamePlayer { GameId = gameId, PlayerId = player.Id };
                gamePlayers.Add(gamePlayer);
            }
            Ctx.GamesPlayers.AddRange(gamePlayers);
            Ctx.SaveChanges();
        }

        public List<Player> Players(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    join p in Ctx.Players on gp.PlayerId equals p.Id
                    where gp.GameId == gameId
                    select p).ToList();
        }

        public int PlayersNumber(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId
                    select gp.Id).Count();
        }

        public List<int> BotsId(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    join p in Ctx.Players on gp.PlayerId equals p.Id
                    where gp.GameId == gameId && p.Bot
                    select p.Id).ToList();
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
            return (from gp in Ctx.GamesPlayers
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.PlayerId == playerId && !gp.ViewFinished && !gp.Turn
                    orderby g.PlayedDate descending
                    select g).AsNoTracking().ToList();
        }

        public List<Game> GamesWon(int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.PlayerId == playerId && gp.ViewFinished && gp.Win == true
                    orderby g.PlayedDate descending
                    select g).AsNoTracking().ToList();
        }

        public List<Game> GamesLost(int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.PlayerId == playerId && gp.ViewFinished && gp.Win == false
                    orderby g.PlayedDate descending
                    select g).AsNoTracking().ToList();
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

            return (from gp in Ctx.GamesPlayers
                    join gnh in idGamesNoHuman on gp.GameId equals gnh
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.PlayerId == playerId && !gp.ViewFinished && g.Status != GameStatus.SingleFinished && g.Status != GameStatus.SingleInProgress
                    orderby g.PlayedDate
                    select g).ToList();
        }

        public List<Game> MultiGamesAgainstAtLeast1HumanToPlay(int playerId)
        {
            var firstFilter = (from gp in Ctx.GamesPlayers
                               join g in Ctx.Games on gp.GameId equals g.Id
                               join p in Ctx.Players on gp.PlayerId equals p.Id
                               where !p.Bot && p.Id != playerId
                               select g).Distinct().AsNoTracking();

            return (from gp in Ctx.GamesPlayers
                    join g in firstFilter on gp.GameId equals g.Id
                    where gp.PlayerId == playerId && (gp.Turn || g.Status == GameStatus.Finished && !gp.ViewFinished) && g.Status != GameStatus.SingleFinished && g.Status != GameStatus.SingleInProgress
                    orderby g.PlayedDate
                    select g).AsNoTracking().ToHashSet().ToList();
        }

        public List<Game> GamesWithNotReadMessages(int playerId)
        {
            return (from c in Ctx.Chats
                    join gp in Ctx.GamesPlayers on c.GamePlayerId equals gp.Id
                    join game in Ctx.Games on gp.GameId equals game.Id
                    join gp2 in Ctx.GamesPlayers on game.Id equals gp2.GameId
                    join p in Ctx.Players on gp2.PlayerId equals p.Id
                    where p.Id == playerId && c.Date > gp2.LatestReadMessage
                    select game).Distinct().ToList();
        }

        public int FirstIdMultiGameToPlay(int playerId)
        {
            Game game = MultiGamesAgainstAtLeast1HumanToPlay(playerId).FirstOrDefault();
            return game != null ? game.Id : 0;
        }

        public List<Game> SingleGamesFinished(int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.PlayerId == playerId && g.Status == GameStatus.SingleFinished
                    orderby g.PlayedDate descending
                    select g).AsNoTracking().ToList();
        }

        public IQueryable<GamePlayer> List()
        {
            return from gp in Ctx.GamesPlayers
                   select gp;
        }

        public List<Game> SingleGamesInProgress(int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.PlayerId == playerId && g.Status == GameStatus.SingleInProgress
                    orderby g.PlayedDate
                    select g).AsNoTracking().ToList();
        }

        public List<Game> Games(int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.PlayerId == playerId
                    orderby g.CreateDate
                    select g).AsNoTracking().ToList();
        }

        public Player PlayerTurn(int gameId)
        {
            Player player = (from gp in Ctx.GamesPlayers
                             join p in Ctx.Players on gp.PlayerId equals p.Id
                             where gp.GameId == gameId && gp.Turn
                             select p).FirstOrDefault();
            if (player == null)
            {
                player = (from gp in Ctx.GamesPlayers
                          join p in Ctx.Players on gp.PlayerId equals p.Id
                          where gp.GameId == gameId
                          select p).FirstOrDefault();
            }
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
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.PlayerId == playerId
                    select gp.PreviouslyDraw).FirstOrDefault();
        }

        public bool IsViewFinished(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers.AsNoTracking()
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            return gamePlayer?.ViewFinished ?? true;
        }

        public bool IsAllLoosersViewFinished(int gameId)
        {
            int id = (from gp in Ctx.GamesPlayers.AsNoTracking()
                      join p in Ctx.Players.AsNoTracking() on gp.PlayerId equals p.Id
                      where gp.GameId == gameId && gp.Win != true && !gp.ViewFinished && !p.Bot
                      select gp.Id).FirstOrDefault();

            return id == 0 ? true : false;
        }

        public void SetViewFinished(int gameId, int playerId)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.ViewFinished = true;
            Ctx.SaveChanges();
        }

        public bool IsAllSkip(int gameId)
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
        /// <param name="skip">true : le joueur a passé son tour</param>
        public void SetSkip(int gameId, int playerId, bool skip)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.PreviouslyPass = skip;
            Ctx.SaveChanges();
        }

        public bool IsSkip(int gameId, int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.PlayerId == playerId
                    select gp.PreviouslyPass).FirstOrDefault();
        }

        public bool? GetWon(int gameId, int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.PlayerId == playerId
                    select gp.Win).FirstOrDefault();
        }

        public void SetWon(int gameId, int playerId, bool won = true)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            gamePlayer.Win = won;
            Ctx.SaveChanges();
        }

        /// <summary>
        /// indique si tous les joueurs de la partie sont des bots ou non
        /// </summary>
        /// <param name="gameId">Id de la partie</param>
        /// <param name="excludeId">Id du joueur à éventuellement exclure</param>
        /// <returns>true si tous les joueurs sont des bots</returns>
        public bool IsAllBots(int gameId, int excludeId = 0)
        {
            var ids = from gp in Ctx.GamesPlayers
                      join p in Ctx.Players on gp.PlayerId equals p.Id
                      where gp.GameId == gameId && gp.PlayerId != excludeId
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
        public int LastPlayerIdInRound(int gameId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId
                    orderby gp.Id descending
                    select gp.PlayerId).FirstOrDefault();
        }

        /// <summary>
        /// retourne la liste des joueurs à jouer dans le tour après le joueur renseigné
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <param name="playerId">Id du joueur à renseigner</param>
        /// <returns></returns>
        public List<int> NextPlayersIdInTurn(int gameId, int playerId)
        {
            int gamePlayerId = Details(gameId, playerId).Id;
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.Id > gamePlayerId
                    select gp.PlayerId).ToList();
        }

        /// <summary>
        /// retourne la liste de tous les joueurs à jouer après le joueur renseigné
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <param name="playerId">Id du joueur à renseigner</param>
        /// <returns></returns>
        public List<int> NextPlayersId(int gameId, int playerId)
        {
            List<int> playersId = NextPlayersIdInTurn(gameId, playerId);
            int gamePlayerId = Details(gameId, playerId).Id;
            playersId.AddRange((from gp in Ctx.GamesPlayers
                                where gp.GameId == gameId && gp.Id < gamePlayerId
                                select gp.PlayerId).ToList());

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

        public string GetMemo(int gameId, int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.PlayerId == playerId
                    select gp.Memo).FirstOrDefault();
        }

        public int Delete(IQueryable<int> gamesIdToDelete)
        {
            var result = from gp in Ctx.GamesPlayers
                         where gamesIdToDelete.Contains(gp.GameId)
                         select gp;

            Ctx.GamesPlayers.RemoveRange(result);
            return Ctx.SaveChanges();
        }

        public List<GamePlayer> ListBotsTurn()
        {
            return (from gp in Ctx.GamesPlayers
                    join p in Ctx.Players on gp.PlayerId equals p.Id
                    join g in Ctx.Games on gp.GameId equals g.Id
                    where gp.Turn && p.Bot && g.Status != GameStatus.Finished
                    select gp).AsNoTracking().ToList();
        }

        public IQueryable<int> GamesId(int playerId)
        {
            return from gp in Ctx.GamesPlayers
                   where gp.PlayerId == playerId
                   select gp.GameId;
        }

        public DateTime GetLatestReadMessage(int gameId, int playerId)
        {
            return (from gp in Ctx.GamesPlayers
                    where gp.GameId == gameId && gp.PlayerId == playerId
                    select gp.LatestReadMessage).FirstOrDefault();
        }

        public void SetDateLatestReadMessage(int playerId, int gameId, DateTime date)
        {
            GamePlayer gamePlayer = (from gp in Ctx.GamesPlayers
                                     where gp.GameId == gameId && gp.PlayerId == playerId
                                     select gp).FirstOrDefault();

            if (gamePlayer != null)
            {
                gamePlayer.LatestReadMessage = date;
                Ctx.SaveChanges();
            }
        }
    }
}
