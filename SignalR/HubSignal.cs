using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    [Authorize]
    public class HubSignal : Hub
    {
        public async Task SendMessageSent(int gameId)
        {
            await Clients.Others.SendAsync("ReceiveMessageSent", gameId);
        }

        public async Task SendChrominoPlayed(int gameId, object chrominoPlayed)
        {
            await Clients.Others.SendAsync("ReceiveChrominoPlayed", gameId, chrominoPlayed );
        }

        public async Task SendTurnSkipped(int gameId)
        {
            await Clients.Others.SendAsync("ReceiveTurnSkipped", gameId);
        }

        public async Task SendChrominoDrawn(int gameId)
        {
            await Clients.Others.SendAsync("ReceiveChrominoDrawn", gameId);
        }

        public async Task SendBotChrominoPlayed(int gameId, object chrominoPlayed, bool isDrawn)
        {
            await Clients.Others.SendAsync("ReceiveBotChrominoPlayed", gameId, chrominoPlayed, isDrawn);
        }

        public async Task SendBotTurnSkipped(int gameId, bool isDrawn)
        {
            await Clients.Others.SendAsync("ReceiveBotTurnSkipped", gameId, isDrawn);
        }

    }
}
