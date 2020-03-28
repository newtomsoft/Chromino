#if DEBUG

using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tool;

namespace Data.Core
{
    public partial class GameCore
    {
        private static readonly Random Random = new Random();

        public void BeginGameTestDebug()
        {
            new PictureFactory(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            FillHandPlayersTestDebug();
            ChangePlayerTurn();
        }

        private void FillHandTestDebug(GamePlayer gamePlayer, bool bot = false)
        {
            if (bot)
            {
                GameChrominoDal.StackTestToHand(GameId, gamePlayer.PlayerId, 75);
                GameChrominoDal.StackTestToHand(GameId, gamePlayer.PlayerId, 75);
                GameChrominoDal.StackTestToHand(GameId, gamePlayer.PlayerId, 75);
                GameChrominoDal.StackTestToHand(GameId, gamePlayer.PlayerId, 75);
            }
            else
            {
                GameChrominoDal.StackTestToHand(GameId, gamePlayer.PlayerId, 7);
                GameChrominoDal.StackTestToHand(GameId, gamePlayer.PlayerId, 67);
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