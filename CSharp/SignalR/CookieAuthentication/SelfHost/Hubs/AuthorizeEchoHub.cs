using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using ChatServiceLayer.Shared;
using Newtonsoft.Json;
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
        public override async Task OnConnected()
        {
            var history = _GetChatHistory();
            Clients.Caller.onConnectedHistory(history);

            await Clients.All.onConnected(_GetSender());
        }

        public void Echo()
        {
            var sender = _GetSender();
            Clients.All.onEcho(sender);
        }

        public void CreateGroup(ConversationGroup dto)
        {
            var withId = _GetOrAddConversationGroup(dto);

            foreach (var id in withId.Users)
            {
                Clients.User(id).onCreatedGroup(withId);
            }
        }

        public void SendChatAll(string message)
        {
            //Protoype: Needs persistence and general updated handling...
            var sender = _GetSender();
            var allGroup = new ConversationGroup { Id = null, Users = new string[] { } };
            var route = new MessageRoute { Group = allGroup, Sender = sender };
            var msg = _MapMessage(-1, route, message);
            Clients.All.onSendChatAll(msg);
        }

        public void SendChat(MessageSendInfo info)
        {
            var withId = _SaveMessage(info.Route, info.Content);
            var users = withId.Route.Group.Users;
            foreach (var u in users)
            {
                Clients.User(u).onSendChat(withId);
            }
        }

        public void SendTyping(MessageRoute route)
        {
            foreach (var id in route.Group.Users)
            {
                Clients.User(id).onSendTyping(route);
            }
        }

        private string _GetSender() => Context.User.Identity.Name;

        private static Message _MapMessage(int id, MessageRoute route, string content)
        {
            return new Message
            {
                Id = id,
                MessageTime = DateTime.Now,
                Route = route,
                Content = content
            };
        }

        private Message _SaveMessage(MessageRoute route, string content)
        {
            using (var chats = new ChatsContext())
            {
                var added = chats.Messages.Add(new WebHost.Persistence.Message
                {
                    MessageTime = DateTimeOffset.Now,
                    ConversationGroupId = route.Group.Id.Value,
                    Sender = route.Sender,
                    Content = content
                });
                chats.SaveChanges();

                return _MapMessage(added.Id, route, content);
            }
        }

        private static ConversationGroup _GetOrAddConversationGroup(ConversationGroup dto)
        {
            using (var context = new ChatsContext())
            {
                var usersJson = _Serialize(dto.Users);
                var found = context.ConversationGroups.FirstOrDefault(x => x.Name == dto.Name && x.UsersJson == usersJson);
                if (found != null)
                {
                    dto.Id = found.Id;
                }
                else
                {
                    var added = context.ConversationGroups.Add(new WebHost.Persistence.ConversationGroup
                    {
                        Name = dto.Name,
                        UsersJson = usersJson
                    });
                    context.SaveChanges();

                    dto.Id = added.Id;
                }
            }

            return dto;
        }

        private ChatHistories _GetChatHistory()
        {
            var sender = _GetSender();
            using (var context = new ChatsContext())
            {
                var userConversations = context.ConversationUsers.Where(x => x.User == sender).ToList();
                var histories = userConversations.Select(uc =>
                {
                    var convGrp = context.ConversationGroups
                        .Include(x => x.Messages)
                        .Include(x => x.UserConversations)
                        .First(x => x.Id == uc.ConversationGroupId);

                    var grp = new ChatServiceLayer.Shared.ConversationGroup
                    {
                        Id = convGrp.Id,
                        Name = convGrp.Name,
                        Users = convGrp.UserConversations.Select(x => x.User).ToArray()
                    };

                    var msgs = convGrp.Messages.Select(m => new ChatServiceLayer.Shared.Message
                    {
                        Id = m.Id,
                        Route = new MessageRoute { Group = grp, Sender = m.Sender },
                        MessageTime = m.MessageTime.DateTime,
                        Content = m.Content
                    }).ToArray();
                    return new ChatHistory { ConversationGroup = grp, Messages = msgs };
                }).ToArray();

                return new ChatHistories { Histories = histories };
            }
        }

        private static string _Serialize<T>(T data) => JsonConvert.SerializeObject(data, Formatting.None);
        
        private static T _Deserialize<T>(string jsonData)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            return JsonConvert.DeserializeObject<T>(jsonData, settings);
        }
    }
}
