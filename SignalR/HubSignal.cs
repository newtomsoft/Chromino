using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    [Authorize]
    public class HubSignal : Hub
    {
        public async Task AddToGroup(string guid) => await Groups.AddToGroupAsync(Context.ConnectionId, guid);

        public async Task SendMessageSent(string guid) => await Clients.OthersInGroup(guid).SendAsync("ReceiveMessageSent");

        public async Task SendChrominoPlayed(string guid, object chrominoPlayed) => await Clients.OthersInGroup(guid).SendAsync("ReceiveChrominoPlayed", chrominoPlayed);

        public async Task SendTurnSkipped(string guid) => await Clients.OthersInGroup(guid).SendAsync("ReceiveTurnSkipped");

        public async Task SendChrominoDrawn(string guid) => await Clients.OthersInGroup(guid).SendAsync("ReceiveChrominoDrawn");

        public async Task SendBotChrominoPlayed(string guid, object chrominoPlayed, bool isDrawn) => await Clients.OthersInGroup(guid).SendAsync("ReceiveBotChrominoPlayed", chrominoPlayed, isDrawn);

        public async Task SendBotTurnSkipped(string guid, bool isDrawn) => await Clients.OthersInGroup(guid).SendAsync("ReceiveBotTurnSkipped", isDrawn);

    }
}
