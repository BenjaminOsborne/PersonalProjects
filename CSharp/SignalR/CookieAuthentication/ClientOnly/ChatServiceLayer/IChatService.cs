using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public interface IChatService
    {
        Task Login(string username, string password);

        IObservable<ImmutableList<ConverationGroup>> GetObservableUsers();
        IObservable<ImmutableList<Message>> GetObservableMessages(ConverationGroup group);
        IObservable<Unit> GetObservableTyping(ConverationGroup group);

        Task SendGlobalMessage(string message);
        Task SendChat(MessageRoute route, string content);
        Task SendTyping(MessageRoute route);
    }
}
