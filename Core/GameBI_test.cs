#if DEBUG

using ChrominoBI;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tool;

namespace Data.Core
{
    public partial class GameBI
    {
        private static readonly Random Random = new Random();

        public void BeginGameTest(bool playFirstChromino = false)
        {
            new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            FillHandPlayersTest();
            ChrominoInGame chrominoInGame = ChrominoInGameDal.FirstToGame(GameId);
            PlayerBI playerBI = new PlayerBI(Ctx, Env, GameId, 0);
            if (playFirstChromino)
                playerBI.Play(chrominoInGame);
            playerBI.ChangePlayerTurn();
        }

        private void FillHandTest(GamePlayer gamePlayer, bool bot = false)
        {
            if (bot)
            {
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 75);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 66);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 55);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 56);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 57);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 58);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 59);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 60);
            }
            else
            {
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 66);
            }
        }

        private void FillHandPlayersTest()
        {
            FillHandTest(GamePlayers[0]);
            FillHandTest(GamePlayers[1], true);
        }

        public async Task TestAsync(int gameId)
        {
            for (int i = 0; i < 200; i++)
            {
                SquareDal squareDal = new SquareDal(Ctx);
                List<Square> squares = new List<Square>();
                for (int j = 0; j < 10; j++)
                {
                    int x = Random.Next(65);
                    int y = Random.Next(65);
                    int maxColorNumber = Enum.GetNames(typeof(ColorCh)).Length;
                    int colorIndex = Random.Next(1, maxColorNumber);
                    ColorCh color = (ColorCh)colorIndex;
                    Square square = new Square { Color = color, X = x, Y = y };
                    squares.Add(square);
                }
                squareDal.AddTestDebug(squares, gameId);
                await Task.Delay(200);
            }
        }
    }
}

#endif