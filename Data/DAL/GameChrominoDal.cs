using Data.Core;
using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class GameChrominoDal
    {
        private readonly Context Ctx;
        private static readonly Random Random = new Random();
        public GameChrominoDal(Context context)
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

        public int InGame(int gameId)
        {
            int nbChrominos = (from cg in Ctx.ChrominosInGame
                               where cg.GameId == gameId
                               select cg).Count();

            return nbChrominos;
        }

        /// <summary>
        /// Nombre total de chrominos dans toutes les mains des joueurs
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        public int InHands(int gameId)
        {
            int nbChrominos = (from ch in Ctx.ChrominosInHand
                               where ch.GameId == gameId
                               select ch).Count();

            return nbChrominos;
        }

        /// <summary>
        /// Nombre de chrominos dans la main du joueur
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <param name="playerId">Id du joueur</param>
        /// <returns></returns>
        public int InHand(int gameId, int playerId)
        {
            int nbChrominos = (from ch in Ctx.ChrominosInHand
                               where ch.GameId == gameId && ch.PlayerId == playerId
                               select ch).Count();

            return nbChrominos;
        }

        /// <summary>
        /// Nombre de chrominos dans la pioche
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        public int InStack(int gameId)
        {
            int nbChrominos = (from c in Ctx.Chrominos
                               select c).Count();

            return nbChrominos - InHands(gameId) - InGame(gameId);
        }

        /// <summary>
        /// Pioche un chromino
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <param name="playerId">id du joueur</param>
        /// <returns>id du chromino pioché. 0 si plus de chromino dans la pioche</returns>
        public int StackToHand(int gameId, int playerId)
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

        public void FirstRandomToGame(int gameId)
        {
            List<Chromino> chrominosCameleon = (from c in Ctx.Chrominos
                                                where c.SecondColor == Color.Cameleon
                                                select c).AsNoTracking().ToList();

            Chromino chromino = chrominosCameleon[Random.Next(chrominosCameleon.Count)];
            Orientation orientation = (Orientation)Random.Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);
            Coordinate coordinate = new Coordinate(0, 0).GetPreviousCoordinate(orientation);
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chromino.Id,
                XPosition = coordinate.X,
                YPosition = coordinate.Y,
                Orientation = orientation,
            };
            Ctx.ChrominosInGame.Add(chrominoInGame);
            new SquareDal(Ctx).PlayChromino(chrominoInGame, null);
        }

        public Chromino FirstChromino(int gameId, int playerId)
        {
            int chrominoId = (from ch in Ctx.ChrominosInHand
                              where ch.GameId == gameId && ch.PlayerId == playerId
                              select ch.ChrominoId).FirstOrDefault();

            Chromino chromino = (from c in Ctx.Chrominos
                                 where c.Id == chrominoId
                                 select c).FirstOrDefault();

            return chromino;
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


        public int PlayerNumberChrominos(int gameId, int playerId)
        {
            int chrominos = (from ch in Ctx.ChrominosInHand
                             where ch.GameId == gameId && ch.PlayerId == playerId
                             select ch).Count();

            return chrominos;
        }

        /// <summary>
        /// tous les chrominosInGame joués du jeu
        /// le 1er placé aléatoirement n'est pas inclus
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public List<ChrominoInGame> ChrominosInGamePlayed(int gameId)
        {
            List<ChrominoInGame> chrominosInGame = (from cg in Ctx.ChrominosInGame
                                                    where cg.GameId == gameId && cg.Move != 0
                                                    orderby cg.Move descending
                                                    select cg).AsNoTracking().ToList();

            return chrominosInGame;
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

        public void ChangePositions(int gameId, int playerId, List<byte> positions)
        {
            List<ChrominoInHand> chrominosInHand = (from ch in Ctx.ChrominosInHand
                                                    where ch.GameId == gameId && ch.PlayerId == playerId
                                                    orderby ch.Position
                                                    select ch).ToList();

            List<byte> copyPositions = new List<byte>(positions);
            byte[] pos = new byte[positions.Count];
            byte value = 1;
            while (positions.Count > 0)
            {
                byte min = positions.Min();
                int index = copyPositions.IndexOf(min);
                positions.Remove(min);
                pos[index] = value++;
            }
            for (int i = 0; i < pos.Count(); i++)
                chrominosInHand[i].Position = pos[i];

            Ctx.SaveChanges();
        }
    }
}
