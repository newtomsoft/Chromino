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

        public GameChromino Details(int gameId, int chrominoId)
        {
            GameChromino gameChromino = (from gc in Ctx.GamesChrominos
                                         where gc.GameId == gameId && gc.ChrominoId == chrominoId
                                         select gc).FirstOrDefault();

            return gameChromino;
        }

        public int StatusNumber(int gameId, ChrominoStatus status)
        {
            int nbChrominos = (from gc in Ctx.GamesChrominos
                               where gc.GameId == gameId && gc.State == status
                               select gc).Count();

            return nbChrominos;
        }

        public void Add(int gameId)
        {
            ChrominoDal chrominoDal = new ChrominoDal(Ctx);
            List<Chromino> chrominos = chrominoDal.List();
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
            Ctx.GamesChrominos.AddRange(chrominosGames);
            Ctx.SaveChanges();
        }

        public GameChromino ChrominoFromStackToHandPlayer(int gameId, int playerId)
        {
            GameChromino gameChromino = (from gc in Ctx.GamesChrominos
                                         where gc.GameId == gameId && gc.State == ChrominoStatus.InStack
                                         orderby Guid.NewGuid()
                                         select gc).FirstOrDefault();

            if (gameChromino != null)
            {
                gameChromino.PlayerId = playerId;
                gameChromino.State = ChrominoStatus.InPlayer;
                Ctx.SaveChanges();
            }
            return gameChromino;
        }

        public GameChromino RandomFirstChromino(int gameId)
        {
            GameChromino chromino = (from gc in Ctx.GamesChrominos
                                     join c in Ctx.Chrominos on gc.ChrominoId equals c.Id
                                     where gc.GameId == gameId && c.SecondColor == Color.Cameleon
                                     orderby Guid.NewGuid()
                                     select gc).FirstOrDefault();

            chromino.Orientation = (Orientation)new Random().Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);

            Ctx.SaveChanges();
            return chromino;
        }

        public GameChromino RandomChrominoForGame(int gameId)
        {
            GameChromino chromino = (from gc in Ctx.GamesChrominos
                                     where gc.GameId == gameId && gc.State == ChrominoStatus.InStack
                                     orderby Guid.NewGuid()
                                     select gc).FirstOrDefault();

            chromino.State = ChrominoStatus.InGame;
            chromino.Orientation = (Orientation)new Random().Next(1, Enum.GetValues(typeof(Orientation)).Length + 1);
            Ctx.SaveChanges();
            return chromino;
        }

        public List<GameChromino> PlayerList(int gameId, int playerId)
        {
            List<GameChromino> chrominos = (from gc in Ctx.GamesChrominos
                                            where gc.GameId == gameId && gc.PlayerId == playerId && gc.State == ChrominoStatus.InPlayer
                                            orderby Guid.NewGuid()
                                            select gc).ToList();

            return chrominos;
        }

        public List<GameChromino> ChrominosByPriority(int gameId, int playerId)
        {
            List<GameChromino> chrominos = (from gc in Ctx.GamesChrominos
                                            join c in Ctx.Chrominos on gc.ChrominoId equals c.Id
                                            where gc.GameId == gameId && gc.PlayerId == playerId && gc.State == ChrominoStatus.InPlayer
                                            orderby c.Points, c.SecondColor
                                            select gc).ToList();

            return chrominos;
        }


        public int PlayerNumberChrominos(int gameId, int playerId)
        {
            int chrominos = (from gc in Ctx.GamesChrominos
                             where gc.GameId == gameId && gc.PlayerId == playerId && gc.State == ChrominoStatus.InPlayer
                             select gc).Count();


            return chrominos;
        }
    }
}
