using Data.Core;
using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public partial class ChrominoInGameDal
    {
        private readonly Context Ctx;
        private static readonly Random Random = new Random();
        public ChrominoInGameDal(Context context)
        {
            Ctx = context;
        }

        public ChrominoInGame Details(int gameId, int chrominoId)
        {
            ChrominoInGame ChrominoInGame = (from ch in Ctx.ChrominosInGame
                                             where ch.GameId == gameId && ch.ChrominoId == chrominoId
                                             select ch).AsNoTracking().FirstOrDefault();

            return ChrominoInGame;
        }

        public int ChrominosNumber(int gameId)
        {
            int nbChrominos = (from cg in Ctx.ChrominosInGame
                               where cg.GameId == gameId
                               select cg).Count();

            return nbChrominos;
        }

        /// <summary>
        /// Nombre de chrominos dans la pioche
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        public int InStack(int gameId)
        {
            ChrominoInHandDal chrominoInHandDal = new ChrominoInHandDal(Ctx);

            int nbChrominos = (from c in Ctx.Chrominos
                               select c).Count();

            return nbChrominos - chrominoInHandDal.ChrominosNumber(gameId) - ChrominosNumber(gameId);
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
            var possiblesChrominosId = chrominosId.Except(chrominosInGameId).Except(chrominosInHandId).ToList();
            int possibleChrominosIdCount = possiblesChrominosId.Count;

            if (possibleChrominosIdCount != 0)
            {
                int chrominoId = possiblesChrominosId[Random.Next(possibleChrominosIdCount)];
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

        public ChrominoInGame FirstRandomToGame(int gameId)
        {
            List<Chromino> chrominosCameleon = (from c in Ctx.Chrominos
                                                where c.SecondColor == ColorCh.Cameleon
                                                select c).AsNoTracking().ToList();

            Chromino chromino = chrominosCameleon[Random.Next(chrominosCameleon.Count)];
            Orientation orientation = (Orientation)Random.Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);
            Coordinate offset = new Coordinate(orientation);
            Coordinate coordinate = new Coordinate(0, 0) - offset;
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chromino.Id,
                XPosition = coordinate.X,
                YPosition = coordinate.Y,
                Orientation = orientation,
            };
            Ctx.ChrominosInGame.Add(chrominoInGame);
            Ctx.SaveChanges();
            return chrominoInGame;
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



        public void Add(ChrominoInGame chrominoInGame)
        {
            chrominoInGame.Id = 0;
            Ctx.ChrominosInGame.Add(chrominoInGame);
            Ctx.SaveChanges();
        }
    }
}
