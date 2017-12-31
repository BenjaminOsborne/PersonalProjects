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
            var msg = _MapMessage(-1, route, message, new MessageReadState[0]);
            Clients.All.onSendChatAll(msg);
        }

        public void SendChat(MessageSendInfo info)
        {
            var withId = _SaveMessage(info.Route, info.Content, new [] { new MessageReadState { User = _GetSender(), HasRead = true } });
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

        private static Message _MapMessage(int id, MessageRoute route, string content, MessageReadState[] readStates)
        {
            return new Message
            {
                Id = id,
                MessageTime = DateTime.Now,
                Route = route,
                Content = content,
                ReadStates = readStates
            };
        }

        private Message _SaveMessage(MessageRoute route, string content, MessageReadState[] readStates)
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

                foreach (var state in readStates)
                {
                    chats.MessageReads.Add(new MessageReads
                    {
                        User = state.User,
                        HasRead = state.HasRead,
                        MessageId = added.Id
                    });
                    chats.SaveChanges();
                }

                return _MapMessage(added.Id, route, content, readStates);
            }
        }

        private static ConversationGroup _GetOrAddConversationGroup(ConversationGroup dto)
        {
            using (var context = new ChatsContext())
            {
                var found = _FindExistingMatchedConversation(dto, context);
                if (found != null)
                {
                    dto.Id = found.Id;
                    return dto;
                }


                var added = context.ConversationGroups.Add(new WebHost.Persistence.ConversationGroup { Name = dto.Name });
                context.SaveChanges();

                foreach (var user in dto.Users)
                {
                    context.ConversationUsers.Add(new ConversationUser { User = user, ConversationGroupId = added.Id });
                    context.SaveChanges();
                }

                dto.Id = added.Id;
                return dto;
            }
        }

        private static WebHost.Persistence.ConversationGroup _FindExistingMatchedConversation(ConversationGroup dto, ChatsContext context)
        {
            if (dto.Users.Any() == false)
            {
                return context.ConversationGroups
                    .Where(x => x.Name == dto.Name)
                    .Include(x => x.UserConversations)
                    .FirstOrDefault(x => x.UserConversations.Count == 0);
            }

            var userData = dto.Users.Select(user =>
            {
                var matches = context.ConversationUsers.Where(x => x.User == user).Select(x => x.ConversationGroupId);
                return new {user, matches = new HashSet<int>(matches)};
            }).ToArray();

            var foundInAll = userData[0].matches.Where(m => userData.Skip(1).All(s => s.matches.Contains(m))).ToArray();
            var alsoMatchName = foundInAll.Select(p => context.ConversationGroups.First(c => c.Id == p))
                .FirstOrDefault(x => x.Name == dto.Name);
            return alsoMatchName;
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

                    //TODO: add Msg read status
                    var msgs = convGrp.Messages.Select(m =>
                    {
                        var readStates = context.MessageReads
                            .Where(r => r.MessageId == m.Id)
                            .Select(x => new MessageReadState { User = x.User, HasRead = x.HasRead })
                            .ToArray();
                        return new ChatServiceLayer.Shared.Message
                        {
                            Id = m.Id,
                            Route = new MessageRoute {Group = grp, Sender = m.Sender},
                            MessageTime = m.MessageTime.DateTime,
                            Content = m.Content,
                            ReadStates = readStates
                        };
                    }).ToArray();
                    return new ChatHistory { ConversationGroup = grp, Messages = msgs };
                }).ToArray();

                return new ChatHistories { Histories = histories };
            }
        }
    }
}
