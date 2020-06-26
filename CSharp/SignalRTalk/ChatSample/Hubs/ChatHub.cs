using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatSample.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("onConnected", "You have connected");
            await Clients.Others.SendAsync("onConnected", "A new player has joined");
        }

        public async Task Send(string name, string message)
        {
            await Clients.Others.SendAsync("broadcastMessage", name, message);
        }
    }
}