using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public class ChatService : IChatService
    {
        public async Task<bool> Login(string userName, string password)
        {
            return true;
        }

        public IObservable<ImmutableList<User>> GetObservableUsers()
        {
            return Observable.Empty<ImmutableList<User>>();
        }
    }
}
