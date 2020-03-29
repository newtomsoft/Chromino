using Data.Core;
using Data.Enumeration;
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

        public ChrominoInHand InHandDetails(int gameId, int chrominoId)
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



        public Chromino FirstChromino(int gameId, int playerId)
        {
            Chromino chromino = (from ch in Ctx.ChrominosInHand
                                 join c in Ctx.Chrominos on ch.ChrominoId equals c.Id
                                 where ch.GameId == gameId && ch.PlayerId == playerId
                                 select c).FirstOrDefault();

            return chromino;
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
            var chrominosInGameId = Ctx.ChrominosInGame.Where(c => c.GameId == gameId).Select(c => c.ChrominoId);
            var chrominosInHandId = Ctx.ChrominosInHand.Where(c => c.GameId == gameId).Select(c => c.ChrominoId);
            var possibleChrominosId = chrominosId.Except(chrominosInGameId).Except(chrominosInHandId).ToList();
            int possibleChrominosIdCount = possibleChrominosId.Count;
            if (possibleChrominosIdCount != 0)
            {
                int chrominoId = possibleChrominosId[Random.Next(possibleChrominosIdCount)];
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

        public int LastChrominoIdInHand(int gameId, int playerId)
        {
            int chrominoId = (from chl in Ctx.ChrominosInHandLast
                              where chl.GameId == gameId && chl.PlayerId == playerId
                              select chl.ChrominoId).FirstOrDefault();

            return chrominoId;
        }

        public void UpdateLastChrominoInHand(int gameId, int playerId, int chrominoId)
        {
            ChrominoInHandLast chrominohl = (from chl in Ctx.ChrominosInHandLast
                                             where chl.GameId == gameId && chl.PlayerId == playerId
                                             select chl).FirstOrDefault();

            if (chrominohl == null)
            {
                chrominohl = new ChrominoInHandLast { GameId = gameId, PlayerId = playerId, ChrominoId = chrominoId };
                Ctx.Add(chrominohl);
                Ctx.SaveChanges();
            }
            else if (chrominohl.ChrominoId != chrominoId)
            {
                chrominohl.ChrominoId = chrominoId;
                Ctx.SaveChanges();
            }
        }

        public void DeleteLastChrominoInHand(int gameId, int playerId)
        {
            ChrominoInHandLast chromino = (from chl in Ctx.ChrominosInHandLast
                                           where chl.GameId == gameId && chl.PlayerId == playerId
                                           select chl).FirstOrDefault();

            if(chromino != null)
                Ctx.ChrominosInHandLast.Remove(chromino);
            Ctx.SaveChanges();
        }

        public List<int> PlayersIdWithOneChrominoKnown(int gameId, int playerIdToExclude)
        {
            List<int> playersId = (from chl in Ctx.ChrominosInHandLast
                                   where chl.GameId == gameId && chl.PlayerId != playerIdToExclude
                                   select chl.PlayerId).ToList();

            return playersId;
        }

        public List<Chromino> ListLastChrominoIdInHand(int gameId, int playerIdToExclude)
        {
            List<Chromino> chrominos = (from chl in Ctx.ChrominosInHandLast
                                        join c in Ctx.Chrominos on chl.ChrominoId equals c.Id
                                        where chl.GameId == gameId && chl.PlayerId != playerIdToExclude
                                        select c).AsNoTracking().ToList();

            return chrominos;
        }
    }
}
