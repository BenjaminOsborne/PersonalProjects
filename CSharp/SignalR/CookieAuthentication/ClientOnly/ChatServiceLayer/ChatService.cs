using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ChatServiceLayer.Shared;
using JetBrains.Annotations;

namespace ChatServiceLayer
{
    public class ChatService : IChatService
    {
        private readonly ChatClient _client;
        private readonly ChatModel _chatModel = new ChatModel();

        public ChatService()
        {
            var url = "http://localhost:8080/";

            var logger = new WritterLogger(Console.Out);
            _client = new ChatClient(url, logger, fnGroupExistsWithUsers: users => _chatModel.ConversationExists(users));

            _client.GetObservableConversations().Subscribe(u => _chatModel.AddConversation(u));
            _client.GetObservableMessages().Subscribe(m => _chatModel.AddMessage(m));
        }

        public void Dispose() => _client.Dispose();

        public async Task<bool> Login(string username, string password)
        {
            try
            {
                await _client.InitialiseConnection(username, password);
                await _client.RunChatHub(username);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IObservable<ImmutableList<ConversationGroup>> GetObservableConversations()
        {
            return _chatModel.GetObservableConversations();
        }

        public IObservable<ImmutableList<Message>> GetObservableMessages(ConversationGroup group)
        {
            return _chatModel.GetObservableMessages(group);
        }

        public IObservable<string> GetObservableTyping(ConversationGroup group)
        {
            return _client.GetObservableTyping().Where(x => x.Group == group).Select(x => x.Sender);
        }

        public IObservable<ImmutableList<string>> GetObservableAllUsers()
        {
            return _chatModel.GetObservableConversations().Select(g =>
            {
                var users = g.SelectMany(x => x.Users).Distinct();
                return users.OrderBy(x => x).ToImmutableList();
            });
        }

        public async Task SendGlobalMessage(string message) => await _client.SendGlobalMessage(message);

        public async Task SendChat(MessageRoute route, string content)
        {
            var dto = new MessageSendInfo { Route = _CreateRouteDTO(route), Content = content };
            await _client.SendChat(dto);
        }

        public async Task SendTyping(MessageRoute route)
        {
            var dto = _CreateRouteDTO(route);
            await _client.SendTyping(dto);
        }

        [CanBeNull]
        public async Task<bool> CreateGroup(string customName, ImmutableList<string> users)
        {
            if (users.Any() == false)
            {
                return false;
            }

            var exists = _chatModel.ConversationExists(users);
            if (exists)
            {
                return false;
            }

            var dto = new Shared.ConversationGroup { Id = null, Name = customName, Users = users.ToArray() };
            await _client.CreateGroup(dto);
            return true;
        }

        private static Shared.MessageRoute _CreateRouteDTO(MessageRoute route)
        {
            var group = route.Group;
            var groupDTO = new Shared.ConversationGroup { Id = group.Id, Name = group.Name, Users = group.Users.ToArray() };
            return new Shared.MessageRoute { Group = groupDTO, Sender = route.Sender };
        }
    }

    public class ChatModel
    {
        private readonly object _lock = new object();

        private readonly Dictionary<int, Message> _messages = new Dictionary<int, Message>();
        private readonly Subject<ImmutableList<Message>> _subjectMessages = new Subject<ImmutableList<Message>>();

        private readonly HashSet<ConversationGroup> _conversationGroups = new HashSet<ConversationGroup>();
        private readonly Subject<ImmutableList<ConversationGroup>> _subjectConversations = new Subject<ImmutableList<ConversationGroup>>();

        public bool ConversationExists(IEnumerable<string> users)
        {
            var key = ConversationGroup.CreateUsersKey(users);
            lock (_lock)
            {
                return _conversationGroups.Any(x => x.UsersKey.Equals(key));
            }
        }

        public bool AddConversation(ConversationGroup user)
        {
            lock (_lock)
            {
                var added = _conversationGroups.Add(user);
                if (added == false)
                {
                    return false;
                }
                _subjectConversations.OnNext(_GetConversationGroups());
                return true;
            }
        }

        public IObservable<ImmutableList<ConversationGroup>> GetObservableConversations()
        {
            return _subjectConversations.StartWith(_GetConversationGroups());
        }

        public void AddMessage(Message message)
        {
            lock (_lock)
            {
                _messages[message.Id] = message;
                _subjectMessages.OnNext(_GetMessages());
            }
        }

        public IObservable<ImmutableList<Message>> GetObservableMessages(ConversationGroup group)
        {
            return _subjectMessages.StartWith(_GetMessages())
                .Select(x => x.Where(m => m.Route.Group == group).ToImmutableList());
        }

        private ImmutableList<Message> _GetMessages() => _messages.Values.OrderBy(x => x.MessageTime).ToImmutableList();

        private ImmutableList<ConversationGroup> _GetConversationGroups() => _conversationGroups.OrderBy(x => x.Name).ToImmutableList();
    }
}
