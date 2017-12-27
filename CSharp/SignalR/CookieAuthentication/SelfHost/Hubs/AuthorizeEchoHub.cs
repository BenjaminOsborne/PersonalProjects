using System;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        public override Task OnConnected() => Clients.All.onConnected(_CurrentUser());

        public void Echo() => Clients.All.onEcho(_CurrentUser());

        public void BroadcastAll(string message)
        {
            var sender = _CurrentUser();
            var json = _CreateMessage(sender, "All", message);

            Clients.All.onBroadcastAll(json);
        }

        public void BroadcastSpecific(string receiver, string message)
        {
            var sender = _CurrentUser();
            var json = _CreateMessage(sender, receiver, message);
            
            Clients.User(receiver).onBroadcastSpecific(json);
            Clients.User(sender).onBroadcastCallBack(json);
        }

        public void BroadcastTyping(string receiver)
        {
            var sender = _CurrentUser();
            Clients.User(receiver).onBroadcastTyping(sender);
        }

        private string _CurrentUser() => Context.User.Identity.Name;

        private static string _CreateMessage(string sender, string receiver, string message)
        {
            var dto = new ChatServiceLayer.Shared.Message
            {
                MessageId = Guid.NewGuid(),
                MessageTime = DateTime.Now,
                Sender = sender,
                Receiver = receiver,
                Text = message
            };
            return _Serialze(dto);
        }

        private static string _Serialze<T>(T dto) => JsonConvert.SerializeObject(dto, Formatting.None);
    }
}
