using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

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

        public IObservable<ImmutableList<string>> GetObservableUsers()
        {
            return _chatModel.GetObservableUsers();
        }

        public IObservable<ImmutableList<Message>> GetObservableMessages(string sender, string receiver)
        {
            return _chatModel.GetObservableMessages(sender, receiver);
        }

        public async Task SendGlobalMessage(string message) => await _client.SendGlobalMessage(message);

        public async Task SendChat(string receiver, string message) => await _client.SendChat(receiver, message);
    }

    public class ChatModel
    {
        private readonly object _lock = new object();

        private readonly Dictionary<Guid, Message> _messages = new Dictionary<Guid, Message>();
        private readonly Subject<ImmutableList<Message>> _subjectMessages = new Subject<ImmutableList<Message>>();

        private readonly HashSet<string> _users = new HashSet<string>();
        private readonly Subject<ImmutableList<string>> _subjectUsers = new Subject<ImmutableList<string>>();

        public void AddUser(string user)
        {
            lock (_lock)
            {
                _users.Add(user);
                _subjectUsers.OnNext(_GetUsers());
            }
        }

        public IObservable<ImmutableList<string>> GetObservableUsers()
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

        public IObservable<ImmutableList<Message>> GetObservableMessages(string sender, string receiver)
        {
            return _subjectMessages.StartWith(_GetMessages())
                .Select(x => x
                .Where(m =>
                    (m.Sender == sender && m.Receiver == receiver) ||
                    (m.Sender == receiver && m.Receiver == sender)).ToImmutableList());
        }

        private ImmutableList<Message> _GetMessages() => _messages.Values.OrderBy(x => x.MessageTime).ToImmutableList();

        private ImmutableList<string> _GetUsers() => _users.OrderBy(x => x).ToImmutableList();
    }
}
