using System;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using ChatServiceLayer.Shared;
using WebGrease.Css.Extensions;
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
            var allGroup = new ConversationGroup { GroupName = "All", Users = new string[] { }};
            var route = new MessageRoute { Group = allGroup, Sender = sender };

            var dto = _CreateMessage(route, message);
            Clients.All.onBroadcastAll(dto);
        }

        public void BroadcastSpecific(MessageSendInfo info)
        {
            var route = info.Route;
            var dto = _CreateMessage(route, info.Content);
            route.Group.Users.ForEach(u => Clients.User(u).onBroadcastSpecific(dto));
        }

        public void BroadcastTyping(MessageRoute route)
        {
            route.Group.Users.ForEach(u => Clients.User(u).onBroadcastTyping(route));
        }

        private string _GetSender() => Context.User.Identity.Name;

        private static Message _CreateMessage(MessageRoute route, string content)
        {
            return new Message
            {
                MessageId = Guid.NewGuid(),
                MessageTime = DateTime.Now,
                Route = route,
                Content = content
            };
        }
    }
}
