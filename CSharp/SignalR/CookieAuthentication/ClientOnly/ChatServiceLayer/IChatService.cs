using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public interface IChatService
    {
        Task Login(string username, string password);

        IObservable<ImmutableList<string>> GetObservableUsers();
        IObservable<ImmutableList<Message>> GetObservableMessages(string sender, string receiver);
        IObservable<Unit> GetObservableTyping(string sender);

        Task SendGlobalMessage(string message);
        Task SendChat(string receiver, string message);
        Task SendTyping(string receiver);
    }
}
