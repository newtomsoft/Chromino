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

        public int Count(int gameId)
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

            return nbChrominos - chrominoInHandDal.ChrominosNumber(gameId) - Count(gameId);
        }

        

        public ChrominoInGame FirstToGame(int gameId)
        {
            List<Chromino> candidatesCameleon = (from c in Ctx.Chrominos
                                                 where c.SecondColor == ColorCh.Cameleon
                                                 select c).AsNoTracking().ToList();

            Chromino chromino = candidatesCameleon[Random.Next(candidatesCameleon.Count)];
            Orientation orientation = (Orientation)Random.Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);
            Coordinate coordinate = -new Coordinate(orientation);
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chromino.Id,
                XPosition = coordinate.X,
                YPosition = coordinate.Y,
                Orientation = orientation,
                Flip = Random.Next(0, 2) == 0 ? false : true,
                PlayerId = null,
            };
            return chrominoInGame;
        }

        /// <summary>
        /// tous les chrominosInGame joués du jeu
        /// le 1er placé aléatoirement n'est pas inclus
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public List<ChrominoInGame> List(int gameId)
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
