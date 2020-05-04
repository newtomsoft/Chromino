using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    public class HubChat : Hub
    {
        public async Task SendMessage()
        {
            await Clients.Others.SendAsync("ReceiveMessage");
        }
    }
}
