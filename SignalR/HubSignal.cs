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
        static public Dictionary<string, List<int>> GameGuid_PlayersId { get; set; }

        public override Task OnConnectedAsync()
        {
            (LoggedPlayersId ??= new List<int>()).Add(int.Parse(Context.UserIdentifier));
            Clients.All.SendAsync("ReceivePlayersLogged", LoggedPlayersId.ToHashSet().ToList());
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            int id = int.Parse(Context.UserIdentifier);
            while (LoggedPlayersId.Remove(id)) ;
            Clients.All.SendAsync("ReceivePlayersLogged", LoggedPlayersId.ToHashSet().ToList());
            if (GameGuid_PlayersId != null)
                foreach (var guid_ids in GameGuid_PlayersId)
                    while (guid_ids.Value.Remove(id))
                        Clients.Group(guid_ids.Key).SendAsync("ReceivePlayersInGame", GameGuid_PlayersId[guid_ids.Key].ToHashSet().ToList());
          
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
            if (!GameGuid_PlayersId.ContainsKey(guid))
                GameGuid_PlayersId.Add(guid, new List<int>());
            GameGuid_PlayersId[guid].Add(int.Parse(Context.UserIdentifier));
            await Groups.AddToGroupAsync(Context.ConnectionId, guid);
            await Clients.Group(guid).SendAsync("ReceivePlayersInGame", GameGuid_PlayersId[guid].ToHashSet().ToList());
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
