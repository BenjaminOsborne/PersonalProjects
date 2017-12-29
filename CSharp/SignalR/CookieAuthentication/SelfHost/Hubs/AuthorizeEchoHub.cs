using System;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using ChatServiceLayer.Shared;
using WebGrease.Css.Extensions;
using WebHost.Persistence;
using ConversationGroup = ChatServiceLayer.Shared.ConversationGroup;
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

        public void Echo()
        {
            var sender = _GetSender();
            Clients.All.onEcho(sender);
        }

        public void EchoGroup(ConversationGroup group)
        {
            group.Users.ForEach(u => Clients.User(u).onEchoGroup(group));
        }

        public void BroadcastAll(string message)
        {
            var sender = _GetSender();
            var allGroup = new ConversationGroup { Users = new string[] { }};
            var route = new MessageRoute { Group = allGroup, Sender = sender };

            var msg = _CreateMessage(route, message);
            Clients.All.onBroadcastAll(msg);
        }

        public void BroadcastSpecific(MessageSendInfo info)
        {
            var route = info.Route;
            var dto = _CreateMessage(route, info.Content);

            using (var chats = new ChatsContext())
            {
                var found = chats.ConversationGroups.Add(new WebHost.Persistence.ConversationGroup()
                {
                    Users = dto.Route.Group.Users
                });
                chats.SaveChanges();

                var found2 = chats.Messages.Add(new WebHost.Persistence.Message
                {
                    MessageTime = DateTimeOffset.Now,
                    ConversationGroupId = found.Id,
                    Sender = dto.Route.Sender,
                    Content = dto.Content
                });
                chats.SaveChanges();
            }

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
