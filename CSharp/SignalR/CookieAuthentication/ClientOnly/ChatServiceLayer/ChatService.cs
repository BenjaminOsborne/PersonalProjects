using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public class ChatService : IChatService
    {
        private readonly ChatClient _client;

        public ChatService()
        {
            var url = "http://localhost:8080/";

            var logger = new WritterLogger(Console.Out);
            _client = new ChatClient(url, logger);
        }

        public void Dispose() => _client.Dispose();

        public async Task Login(string username, string password)
        {
            await _client.InitialiseConnection(username, password);
            await _client.RunChatHub(username);
        }

        public IObservable<ImmutableList<string>> GetObservableUsers()
        {
            var trackedUsers = new HashSet<string>();
            return _client.GetObservableUsers().Select(x =>
            {
                trackedUsers.Add(x);
                return trackedUsers.ToImmutableList();
            });
        }

        public IObservable<ImmutableList<Message>> GetObservableMessages()
        {
            var trackedMessages = ImmutableList<Message>.Empty;
            return _client.GetObservableMessages().Select(x =>
            {
                trackedMessages = trackedMessages.Add(x);
                return trackedMessages;
            });
        }

        public async Task SendGlobalMessage(string message) => await _client.SendGlobalMessage(message);

        public async Task SendChat(string user, string message) => await _client.SendChat(user, message);
    }
}
