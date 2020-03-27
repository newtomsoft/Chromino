using Data.DAL;
using Data.Models;
using System.IO;
using Tool;

namespace Data.Core
{
    public partial class GameCore
    {
        public void BeginGameTest()
        {
            new PictureFactory(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            FillHandPlayersTest();
            ChangePlayerTurn();
        }

        private void FillHandTest(GamePlayer gamePlayer, bool bot = false)
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

        private void FillHandPlayersTest()
        {
            FillHandTest(GamePlayers[0]);
            FillHandTest(GamePlayers[1], true);
        }

    }
}