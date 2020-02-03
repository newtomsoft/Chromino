using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data.Core;
using Data.Enumeration;
using Microsoft.EntityFrameworkCore;

namespace Data.DAL
{
    public class GameChrominoDal
    {
        private readonly DefaultContext Ctx;

        public GameChrominoDal(DefaultContext context)
        {
            Ctx = context;
        }

        public ChrominoInHand Details(int gameId, int chrominoId)
        {
            ChrominoInHand chrominoInHand = (from ch in Ctx.ChrominosInHand
                                             where ch.GameId == gameId && ch.ChrominoId == chrominoId
                                             select ch).FirstOrDefault();

            return chrominoInHand;
        }

        public int InGame(int gameId)
        {
            int nbChrominos = (from cg in Ctx.ChrominosInGame
                               where cg.GameId == gameId
                               select cg).Count();

            return nbChrominos;
        }

        public int InHands(int gameId)
        {
            int nbChrominos = (from ch in Ctx.ChrominosInHand
                               where ch.GameId == gameId
                               select ch).Count();

            return nbChrominos;
        }

        public int InHand(int gameId, int playerId)
        {
            int nbChrominos = (from ch in Ctx.ChrominosInHand
                               where ch.GameId == gameId && ch.PlayerId == playerId
                               select ch).Count();

            return nbChrominos;
        }

        public int InStack(int gameId)
        {
            int nbChrominos = (from c in Ctx.Chrominos
                               select c).Count();

            return nbChrominos - InHands(gameId) - InGame(gameId);
        }

        public int StackToHand(int gameId, int playerId)
        {
            var chrominosId = Ctx.Chrominos.Select(c => c.Id);
            var chrominosInGameId = Ctx.ChrominosInGame.Where(c => c.GameId == gameId).Select(c => c.ChrominoId);
            var chrominosInHandId = Ctx.ChrominosInHand.Where(c => c.GameId == gameId).Select(c => c.ChrominoId);
            var possibleChrominosId = chrominosId.Except(chrominosInGameId).Except(chrominosInHandId);

            int chrominoId = (from id in possibleChrominosId
                              orderby Guid.NewGuid()
                              select id).FirstOrDefault();

            if (chrominoId != 0)
            {
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
            }
            return chrominoId;
        }

        public void StackToGame(Chromino chromino, int gameId, Coordinate coordinate, Orientation orientation)
        {
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chromino.Id,
                XPosition = coordinate.X,
                YPosition = coordinate.Y,
                Orientation = orientation,
            };
            Ctx.ChrominosInGame.Add(chrominoInGame);
            new SquareDal(Ctx).PutChromino(chrominoInGame, true);
        }

        public void FirstRandomToGame(int gameId)
        {
            Chromino chromino = (from c in Ctx.Chrominos
                                 where c.SecondColor == Color.Cameleon
                                 orderby Guid.NewGuid()
                                 select c).AsNoTracking().FirstOrDefault();

            Orientation orientation = (Orientation)new Random().Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);
            Coordinate coordinate = new Coordinate(0, 0).GetPreviousCoordinate(orientation);
            StackToGame(chromino, gameId, coordinate, orientation);
        }

        public List<ChrominoInHand> PlayerList(int gameId, int playerId)
        {
            List<ChrominoInHand> chrominos = (from ch in Ctx.ChrominosInHand
                                              where ch.GameId == gameId && ch.PlayerId == playerId
                                              orderby Guid.NewGuid()
                                              select ch).ToList();

            return chrominos;
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
    }
}
