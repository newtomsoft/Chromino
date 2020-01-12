using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data.Core;
using Data.Enumeration;

namespace Data.DAL
{
    public class GameChrominoDal
    {
        private readonly DefaultContext Ctx;

        public GameChrominoDal(DefaultContext context)
        {
            Ctx = context;
        }

        public int InGame(int gameId)
        {
            int nbChrominos = (from cg in Ctx.Chrominos_Games
                               where cg.GameId == gameId && cg.State == ChrominoStatus.InGame
                               select cg).Count();

            return nbChrominos;
        }

        public int InStack(int gameId)
        {
            int nbChrominos = (from cg in Ctx.Chrominos_Games
                               where cg.GameId == gameId && cg.State == ChrominoStatus.InStack
                               select cg).Count();

            return nbChrominos;
        }

        public void Add(int gameId)
        {
            ChrominoDal chrominoDAL = new ChrominoDal(Ctx);
            List<Chromino> chrominos = chrominoDAL.List();
            List<GameChromino> chrominosGames = new List<GameChromino>();
            foreach (Chromino chromino in chrominos)
            {
                GameChromino chrominoGame = new GameChromino
                {
                    ChrominoId = chromino.Id,
                    GameId = gameId,
                    State = ChrominoStatus.InStack
                };
                chrominosGames.Add(chrominoGame);
            }
            Ctx.Chrominos_Games.AddRange(chrominosGames);
            Ctx.SaveChanges();
        }

        public GameChromino ChrominoFromStackToHandPlayer(int gameId, int playerId)
        {
            GameChromino chrominoGame = (from cg in Ctx.Chrominos_Games
                                          where cg.GameId == gameId && cg.State == ChrominoStatus.InStack
                                          orderby Guid.NewGuid()
                                          select cg).FirstOrDefault();

            if (chrominoGame != null)
            {
                chrominoGame.PlayerId = playerId;
                chrominoGame.State = ChrominoStatus.InPlayer;
                Ctx.SaveChanges();
            }
            return chrominoGame;
        }

        public GameChromino RandomFirstChromino(int gameId)
        {
            GameChromino chromino = (from cg in Ctx.Chrominos_Games
                                      join c in Ctx.Chrominos on cg.ChrominoId equals c.Id
                                      where cg.GameId == gameId && c.SecondColor == Color.Cameleon
                                      orderby Guid.NewGuid()
                                      select cg).FirstOrDefault();

            chromino.Orientation = (Orientation)new Random().Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);

            Ctx.SaveChanges();
            return chromino;
        }

        public GameChromino RandomChrominoForGame(int gameId)
        {
            GameChromino chromino = (from cg in Ctx.Chrominos_Games
                                      where cg.GameId == gameId && cg.State == ChrominoStatus.InStack
                                      orderby Guid.NewGuid()
                                      select cg).FirstOrDefault();

            chromino.State = ChrominoStatus.InGame;
            chromino.Orientation = (Orientation)new Random().Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);
            Ctx.SaveChanges();
            return chromino;
        }

        public List<GameChromino> PlayerList(int gameId, int playerId)
        {
            List<GameChromino> chrominos = (from cg in Ctx.Chrominos_Games
                                             where cg.GameId == gameId && cg.PlayerId == playerId && cg.State == ChrominoStatus.InPlayer
                                             orderby Guid.NewGuid()
                                             select cg).ToList();

            return chrominos;
        }

        public List<GameChromino> PlayerListByPriority(int gameId, int playerId)
        {
            List<GameChromino> chrominos = (from cg in Ctx.Chrominos_Games
                                             join c in Ctx.Chrominos on cg.ChrominoId equals c.Id
                                             where cg.GameId == gameId && cg.PlayerId == playerId && cg.State == ChrominoStatus.InPlayer
                                             orderby c.Points, c.SecondColor
                                             select cg).ToList();

            return chrominos;
        }


        public int PlayerNumberChrominos(int gameId, int playerId)
        {
            int chrominos = (from cg in Ctx.Chrominos_Games
                             where cg.GameId == gameId && cg.PlayerId == playerId && cg.State == ChrominoStatus.InPlayer
                             select cg).Count();


            return chrominos;
        }
    }
}
