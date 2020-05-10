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
        static public List<int> PlayersLogged { get; set; }
        static public Dictionary<string, List<int>> PlayersInGame { get; set; }

        public override Task OnConnectedAsync()
        {
            (PlayersLogged ??= new List<int>()).Add(int.Parse(Context.UserIdentifier));
            Clients.All.SendAsync("ReceivePlayersLogged", PlayersLogged.ToHashSet().ToList());
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            int id = int.Parse(Context.UserIdentifier);
            while (PlayersLogged.Remove(id)) ;
            Clients.All.SendAsync("ReceivePlayersLogged", PlayersLogged.ToHashSet().ToList());
            if (PlayersInGame != null)
                foreach (var guid_ids in PlayersInGame)
                    while (guid_ids.Value.Remove(id))
                        Clients.Group(guid_ids.Key).SendAsync("ReceivePlayersInGame", PlayersInGame[guid_ids.Key].ToHashSet().ToList());
          
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Envoie l'information que le joueur a une page ouverte sur le jeu
        /// </summary>
        /// <param name="guid">Guid du jeu</param>
        /// <returns></returns>
        public async Task SendAddToGame(string guid)
        {
            PlayersInGame ??= new Dictionary<string, List<int>>();
            if (!PlayersInGame.ContainsKey(guid))
                PlayersInGame.Add(guid, new List<int>());
            PlayersInGame[guid].Add(int.Parse(Context.UserIdentifier));
            await Groups.AddToGroupAsync(Context.ConnectionId, guid);
            await Clients.Group(guid).SendAsync("ReceivePlayersInGame", PlayersInGame[guid].ToHashSet().ToList());
        }

        public async Task SendRemoveFromGame(string guid)
        {
            PlayersInGame[guid].Remove(int.Parse(Context.UserIdentifier));
            await Clients.Group(guid).SendAsync("ReceivePlayersInGame", PlayersInGame[guid].ToHashSet().ToList());
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, guid);
        }

        public async Task SendMessageSent(string guid, List<string> playersId)
        {
            await Clients.Users(playersId).SendAsync("ReceiveMessageSent", guid);
        }
        public async Task SendChrominoPlayed(string guid, List<string> playersId, object chrominoPlayed) => await Clients.Users(playersId).SendAsync("ReceiveChrominoPlayed", guid, chrominoPlayed);

        public async Task SendTurnSkipped(string guid, List<string> playersId) => await Clients.Users(playersId).SendAsync("ReceiveTurnSkipped", guid);

        public async Task SendChrominoDrawn(string guid, List<string> playersId) => await Clients.Users(playersId).SendAsync("ReceiveChrominoDrawn", guid);

        public async Task SendBotChrominoPlayed(string guid, List<string> playersId, object chrominoPlayed, bool isDrawn) => await Clients.Users(playersId).SendAsync("ReceiveBotChrominoPlayed", guid, chrominoPlayed, isDrawn);

        public async Task SendBotTurnSkipped(string guid, List<string> playersId, bool isDrawn) => await Clients.Users(playersId).SendAsync("ReceiveBotTurnSkipped", guid, isDrawn);

    }
}
