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
    }
}
