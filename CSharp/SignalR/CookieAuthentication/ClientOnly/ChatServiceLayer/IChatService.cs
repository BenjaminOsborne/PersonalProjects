using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public interface IChatService
    {
        Task Login(string username, string password);

        IObservable<ImmutableList<ConverationGroup>> GetObservableConversations();
        IObservable<ImmutableList<Message>> GetObservableMessages(ConverationGroup group);
        IObservable<string> GetObservableTyping(ConverationGroup group);

        Task SendGlobalMessage(string message);
        Task SendChat(MessageRoute route, string content);
        Task SendTyping(MessageRoute route);
    }
}
