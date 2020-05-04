using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public partial class ChrominoInHandDal
    {
        private readonly Context Ctx;
        private static readonly Random Random = new Random();
        public ChrominoInHandDal(Context context)
        {
            Ctx = context;
        }

        public ChrominoInHand Details(int gameId, int chrominoId)
        {
            ChrominoInHand chrominoInHand = (from ch in Ctx.ChrominosInHand
                                             where ch.GameId == gameId && ch.ChrominoId == chrominoId
                                             select ch).AsNoTracking().FirstOrDefault();

            return chrominoInHand;
        }

        public void DeleteInHand(int gameId, int chrominoId)
        {
            ChrominoInHand chrominoInHand = (from ch in Ctx.ChrominosInHand
                                             where ch.GameId == gameId && ch.ChrominoId == chrominoId
                                             select ch).FirstOrDefault();
            if (chrominoInHand != null)
            {
                Ctx.ChrominosInHand.Remove(chrominoInHand);
                Ctx.SaveChanges();
            }
        }


        /// <summary>
        /// Retourne le premier chromino de la main d'un joueur
        /// </summary>
        /// <param name="gameId">Id de la partie</param>
        /// <param name="playerId">Id du joueur</param>
        /// <returns></returns>
        public Chromino FirstChromino(int gameId, int playerId)
        {
            Chromino chromino = (from ch in Ctx.ChrominosInHand
                                 join c in Ctx.Chrominos on ch.ChrominoId equals c.Id
                                 where ch.GameId == gameId && ch.PlayerId == playerId
                                 select c).FirstOrDefault();

            return chromino;
        }

        public int FirstChrominoId(int gameId, int playerId)
        {
            int chrominoId = (from ch in Ctx.ChrominosInHand
                              join c in Ctx.Chrominos on ch.ChrominoId equals c.Id
                              where ch.GameId == gameId && ch.PlayerId == playerId
                              select c.Id).FirstOrDefault();

            return chrominoId;
        }

        /// <summary>
        /// Nombre de chrominos dans la main du joueur
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <param name="playerId">Id du joueur</param>
        /// <returns></returns>
        public int ChrominosNumber(int gameId, int playerId)
        {
            int nbChrominos = (from ch in Ctx.ChrominosInHand
                               where ch.GameId == gameId && ch.PlayerId == playerId
                               select ch).Count();

            return nbChrominos;
        }

        /// <summary>
        /// Joueurs avec le moins de chrominos en main
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        public List<int> PlayersIdWithMinChrominos(int gameId)
        {
            var playerId_chrominoNb = from ch in Ctx.ChrominosInHand
                                      where ch.GameId == gameId
                                      group ch by ch.PlayerId into g
                                      orderby g.Count()
                                      select new { playerId = g.Key, chrominoNumber = g.Count() };

            var min = playerId_chrominoNb.Min(x => x.chrominoNumber);

            var playerIds = (from p_c in playerId_chrominoNb
                             where p_c.chrominoNumber == min
                             select p_c.playerId).ToList();

            return playerIds;
        }

        /// <summary>
        /// Nombre total de chrominos dans toutes les mains des joueurs
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        public int ChrominosNumber(int gameId)
        {
            int nbChrominos = (from ch in Ctx.ChrominosInHand
                               where ch.GameId == gameId
                               select ch).Count();

            return nbChrominos;
        }

        /// <summary>
        /// Chrominos (ChrominoInHand) dans la main du joueur
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <param name="playerId">Id du joueur</param>
        /// <returns></returns>
        public List<ChrominoInHand> List(int gameId, int playerId)
        {
            List<ChrominoInHand> chrominos = (from ch in Ctx.ChrominosInHand
                                              where ch.GameId == gameId && ch.PlayerId == playerId
                                              select ch).AsNoTracking().ToList();

            return chrominos;
        }

        /// <summary>
        /// Pioche un chromino
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <param name="playerId">id du joueur</param>
        /// <returns>id du chromino pioché. 0 si plus de chromino dans la pioche</returns>
        public int FromStack(int gameId, int playerId)
        {
            var chrominosId = Ctx.Chrominos.Select(c => c.Id);
            var inGameIds = Ctx.ChrominosInGame.Where(c => c.GameId == gameId && c.ChrominoId != null).Select(c => (int)c.ChrominoId);
            var inHandIds = Ctx.ChrominosInHand.Where(c => c.GameId == gameId).Select(c => c.ChrominoId);
            var candidatesId = chrominosId.Except(inGameIds).Except(inHandIds).ToList();
            int candidatesCount = candidatesId.Count;
            if (candidatesCount != 0)
            {
                int chrominoId = candidatesId[Random.Next(candidatesCount)];
                var positions = from ch in Ctx.ChrominosInHand
                                where ch.GameId == gameId && ch.PlayerId == playerId
                                select ch.Position;

                byte maxPosition = 0;
                if (positions.Count() > 0)
                    maxPosition = positions.Max();

                ChrominoInHand chrominoInHand = new ChrominoInHand()
                {
                    PlayerId = playerId,
                    GameId = gameId,
                    ChrominoId = chrominoId,
                    Position = (byte)(maxPosition + 1),
                };
                Ctx.ChrominosInHand.Add(chrominoInHand);
                Ctx.SaveChanges();
                return chrominoId;
            }
            else
            {
                return 0;
            }
        }

        public List<ChrominoInHand> ChrominosByPriority(int gameId, int playerId)
        {
            List<ChrominoInHand> chrominos = (from ch in Ctx.ChrominosInHand
                                              join c in Ctx.Chrominos on ch.ChrominoId equals c.Id
                                              where ch.GameId == gameId && ch.PlayerId == playerId
                                              orderby c.Points, c.SecondColor
                                              select ch).ToList();

            return chrominos;
        }

        public ChrominoInHand GetNewAddedChrominoInHand(int gameId, int playerId)
        {
            ChrominoInHand chromino = (from ch in Ctx.ChrominosInHand
                                       join c in Ctx.Chrominos on ch.ChrominoId equals c.Id
                                       where ch.GameId == gameId && ch.PlayerId == playerId
                                       orderby ch.Position descending
                                       select ch).FirstOrDefault();

            return chromino;
        }


        /// <summary>
        /// Donne la liste des adversaire avec un seul chromino dans leur main
        /// </summary>
        /// <param name="gameId">Id de la partie</param>
        /// <param name="playerIdToExclude">Id du joueur à ne pas prendre en compte</param>
        /// <returns></returns>
        public List<int> PlayersIdWithOneChromino(int gameId, int playerId = 0)
        {
            List<int> playersId = (from chl in Ctx.ChrominosInHand
                                   where chl.GameId == gameId
                                   group chl by chl.PlayerId into g
                                   where g.Count() == 1
                                   select g.Key).ToList();

            if (playerId != 0)
                playersId.Remove(playerId);

            return playersId;
        }

        public int Delete(IQueryable<int> gamesIdToDelete)
        {
            var result = from ch in Ctx.ChrominosInHand
                         where gamesIdToDelete.Contains(ch.GameId)
                         select ch;

            Ctx.ChrominosInHand.RemoveRange(result);
            return Ctx.SaveChanges();
        }
    }
}
