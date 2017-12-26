using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public interface IChatService
    {
        Task Login(string username, string password);

        IObservable<ImmutableList<string>> GetObservableUsers();
        IObservable<ImmutableList<Message>> GetObservableMessages();

        Task SendGlobalMessage(string message);
        Task SendChat(string user, string message);
    }
}
