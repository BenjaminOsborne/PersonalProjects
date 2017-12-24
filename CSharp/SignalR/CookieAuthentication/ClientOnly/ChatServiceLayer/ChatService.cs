using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public class ChatService : IChatService
    {
        public async Task<bool> Login(string username, string password)
        {
            var url = "http://localhost:8080/";

            var logger = new WritterLogger(Console.Out);
            var client = new ChatClient(url, logger);
            await client.InitialiseConnection(username, password);
            return true;
        }

        public IObservable<ImmutableList<User>> GetObservableUsers()
        {
            return Observable.Empty<ImmutableList<User>>();
        }
    }
}
