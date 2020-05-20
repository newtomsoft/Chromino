using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    [Authorize]
    public class HubSignal : Hub
    {
        static public List<int> LoggedPlayersId { get; set; }
        static private Dictionary<string, List<int>> GameGuid_PlayersId { get; set; }
        static private Dictionary<string, string> ContextId_GameGuid { get; set; }

        public override Task OnConnectedAsync()
        {
            (LoggedPlayersId ??= new List<int>()).Add(int.Parse(Context.UserIdentifier));
            Clients.All.SendAsync("ReceivePlayersLogged", LoggedPlayersId.ToHashSet());
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            int playerId = int.Parse(Context.UserIdentifier);
            LoggedPlayersId.Remove(playerId);
            Clients.All.SendAsync("ReceivePlayersLogged", LoggedPlayersId.ToHashSet());

            if (GameGuid_PlayersId != null)
            {
                string guid = ContextId_GameGuid[Context.ConnectionId];
                GameGuid_PlayersId[guid].Remove(playerId);
                Clients.Group(guid).SendAsync("ReceivePlayersInGame", GameGuid_PlayersId[guid].ToHashSet());

            }
            if (ContextId_GameGuid != null)
                ContextId_GameGuid.Remove(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Envoie l'information que le joueur a une page ouverte sur le jeu
        /// </summary>
        /// <param name="guid">Guid du jeu</param>
        /// <returns></returns>
        public async Task SendAddToGame(string guid)
        {
            GameGuid_PlayersId ??= new Dictionary<string, List<int>>();
            GameGuid_PlayersId.TryAdd(guid, new List<int>());
            GameGuid_PlayersId[guid].Add(int.Parse(Context.UserIdentifier));

            ContextId_GameGuid ??= new Dictionary<string, string>();
            ContextId_GameGuid[Context.ConnectionId] = guid;

            await Groups.AddToGroupAsync(Context.ConnectionId, guid);
            await Clients.Group(guid).SendAsync("ReceivePlayersInGame", GameGuid_PlayersId[guid].ToHashSet());
        }

        public async Task SendPrivateMessageSent(string playerId) => await Clients.Users(playerId).SendAsync("ReceivePrivateMessageMessageSent", int.Parse(Context.UserIdentifier));

        public async Task SendChatMessageSent(string guid, List<string> playersId) => await Clients.Users(playersId).SendAsync("ReceiveChatMessageSent", guid);

        public async Task SendChrominoPlayed(string guid, List<string> playersId, object chrominoPlayed) => await Clients.Users(playersId).SendAsync("ReceiveChrominoPlayed", guid, chrominoPlayed);

        public async Task SendTurnSkipped(string guid, List<string> playersId) => await Clients.Users(playersId).SendAsync("ReceiveTurnSkipped", guid);

        public async Task SendChrominoDrawn(string guid, List<string> playersId) => await Clients.Users(playersId).SendAsync("ReceiveChrominoDrawn", guid);

        public async Task SendBotChrominoPlayed(string guid, List<string> playersId, object chrominoPlayed, bool isDrawn) => await Clients.Users(playersId).SendAsync("ReceiveBotChrominoPlayed", guid, chrominoPlayed, isDrawn);

        public async Task SendBotTurnSkipped(string guid, List<string> playersId, bool isDrawn) => await Clients.Users(playersId).SendAsync("ReceiveBotTurnSkipped", guid, isDrawn);

    }
}
