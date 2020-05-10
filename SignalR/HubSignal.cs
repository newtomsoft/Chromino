using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
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
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            PlayersLogged.Remove(int.Parse(Context.UserIdentifier));
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Envoie l'information que le joueur a une page ouverte sur le jeu
        /// </summary>
        /// <param name="guid">Guid du jeu</param>
        /// <returns></returns>
        public async Task SendAddToGame(string guid)
        {
            ((PlayersInGame ??= new Dictionary<string, List<int>>())[guid] ??= new List<int>()).Add(int.Parse(Context.UserIdentifier));
            await Groups.AddToGroupAsync(Context.ConnectionId, guid);
            await Clients.Group(guid).SendAsync("ReceivePlayersInGame", PlayersInGame[guid]);
        }

        public async Task SendRemoveFromGame(string guid)
        {
            PlayersInGame[guid].Remove(int.Parse(Context.UserIdentifier));
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, guid);
            await Clients.Group(guid).SendAsync("ReceivePlayersInGame", PlayersInGame[guid]);
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
