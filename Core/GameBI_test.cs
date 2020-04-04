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

        public void BeginGameTestDebug(bool playFirstChromino = false)
        {
            new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            FillHandPlayersTestDebug();
            ChrominoInGame chrominoInGame = ChrominoInGameDal.FirstToGame(GameId);
            PlayerBI playerBI = new PlayerBI(Ctx, Env, GameId, 0);
            if (playFirstChromino)
                playerBI.Play(chrominoInGame);
            playerBI.ChangePlayerTurn();
        }

        private void FillHandTestDebug(GamePlayer gamePlayer, bool bot = false)
        {
            if (bot)
            {
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 75);
            }
            else
            {
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 7);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 65);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 66);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 67);
                ChrominoInGameDal.StackTestToHand(GameId, gamePlayer.PlayerId, 68);
            }
        }

        private void FillHandPlayersTestDebug()
        {
            FillHandTestDebug(GamePlayers[0]);
            FillHandTestDebug(GamePlayers[1], true);
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