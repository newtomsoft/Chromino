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

        public async Task SendChrominoPlayed(int gameId, string playerName, object chrominoPlayed)
        {
            await Clients.Others.SendAsync("ReceiveChrominoPlayed", gameId, playerName, chrominoPlayed );
        }

        public async Task SendTurnSkipped(int gameId, string playerName)
        {
            await Clients.Others.SendAsync("ReceiveTurnSkipped", gameId, playerName);
        }

        public async Task SendChrominoDrawn(int gameId, string playerName)
        {
            await Clients.Others.SendAsync("ReceiveChrominoDrawn", gameId, playerName);
        }
    }
}
