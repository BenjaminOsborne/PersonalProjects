using System;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Message = ChatServiceLayer.Shared.Message;

namespace Common.Hubs
{
    [Authorize]
    public class AuthorizeEchoHub : Hub
    {
        public override Task OnConnected()
        {
            return Clients.Caller.hubReceived($"{nameof(AuthorizeEchoHub)} Welcome {Context.User.Identity.Name}!");
        }

        public void Echo(string value)
        {
            Clients.Caller.hubReceived(value);
        }
    }

    [Authorize]
    public class ChatHub : Hub
    {
        public override Task OnConnected() => Clients.All.onConnected(_GetSender());

        public void Echo() => Clients.All.onEcho(_GetSender());

        public void BroadcastAll(string message)
        {
            var sender = _GetSender();
            var dto = _CreateMessage(sender, "All", message);

            Clients.All.onBroadcastAll(dto);
        }

        public void BroadcastSpecific(string receiver, string message)
        {
            var sender = _GetSender();
            var dto = _CreateMessage(sender, receiver, message);
            
            Clients.User(receiver).onBroadcastSpecific(dto);
            Clients.User(sender).onBroadcastCallBack(dto);
        }

        public void BroadcastTyping(string receiver)
        {
            var sender = _GetSender();
            Clients.User(receiver).onBroadcastTyping(sender);
        }

        private string _GetSender() => Context.User.Identity.Name;

        private static Message _CreateMessage(string sender, string receiver, string message)
        {
            return new Message
            {
                MessageId = Guid.NewGuid(),
                MessageTime = DateTime.Now,
                Sender = sender,
                Receiver = receiver,
                Text = message
            };
        }
    }
}
