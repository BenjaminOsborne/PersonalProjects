using System;
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

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.Others.SendAsync("onDisconnected", "A player has left");
        }

        public async Task Send(string name, string message)
        {
            await Clients.Others.SendAsync("broadcastMessage", name, message);
        }
    }
}