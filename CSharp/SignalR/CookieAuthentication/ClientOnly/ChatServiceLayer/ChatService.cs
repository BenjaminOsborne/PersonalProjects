using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ChatServiceLayer.Shared;

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
            _client = new ChatClient(url, logger);

            _client.GetObservableUsers().Subscribe(u => _chatModel.AddUser(u));
            _client.GetObservableMessages().Subscribe(m => _chatModel.AddMessage(m));
        }

        public void Dispose() => _client.Dispose();

        public async Task Login(string username, string password)
        {
            await _client.InitialiseConnection(username, password);
            await _client.RunChatHub(username);
        }

        public IObservable<ImmutableList<ConverationGroup>> GetObservableUsers()
        {
            return _chatModel.GetObservableUsers();
        }

        public IObservable<ImmutableList<Message>> GetObservableMessages(ConverationGroup group)
        {
            return _chatModel.GetObservableMessages(group);
        }

        public IObservable<Unit> GetObservableTyping(ConverationGroup group)
        {
            return _client.GetObservableUserTyping().Where(x => x.Group == group).Select(_ => Unit.Instance);
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

        private static Shared.MessageRoute _CreateRouteDTO(MessageRoute route)
        {
            var group = route.Group;
            var groupDTO = new ConversationGroup { Users = group.Users.ToArray() };
            return new Shared.MessageRoute { Group = groupDTO, Sender = route.Sender };
        }
    }

    public class ChatModel
    {
        private readonly object _lock = new object();

        private readonly Dictionary<Guid, Message> _messages = new Dictionary<Guid, Message>();
        private readonly Subject<ImmutableList<Message>> _subjectMessages = new Subject<ImmutableList<Message>>();

        private readonly HashSet<ConverationGroup> _users = new HashSet<ConverationGroup>();
        private readonly Subject<ImmutableList<ConverationGroup>> _subjectUsers = new Subject<ImmutableList<ConverationGroup>>();

        public void AddUser(ConverationGroup user)
        {
            lock (_lock)
            {
                _users.Add(user);
                _subjectUsers.OnNext(_GetUsers());
            }
        }

        public IObservable<ImmutableList<ConverationGroup>> GetObservableUsers()
        {
            return _subjectUsers.StartWith(_GetUsers());
        }

        public void AddMessage(Message message)
        {
            lock (_lock)
            {
                _messages[message.MessageId] = message;
                _subjectMessages.OnNext(_GetMessages());
            }
        }

        public IObservable<ImmutableList<Message>> GetObservableMessages(ConverationGroup group)
        {
            return _subjectMessages.StartWith(_GetMessages())
                .Select(x => x.Where(m => m.Route.Group == group).ToImmutableList());
        }

        private ImmutableList<Message> _GetMessages() => _messages.Values.OrderBy(x => x.MessageTime).ToImmutableList();

        private ImmutableList<ConverationGroup> _GetUsers() => _users.OrderBy(x => x.UsersFlat).ToImmutableList();
    }
}
