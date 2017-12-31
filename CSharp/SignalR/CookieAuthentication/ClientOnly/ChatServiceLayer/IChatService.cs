using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public interface IChatService
    {
        Task<bool> Login(string username, string password);

        IObservable<ImmutableList<ConversationGroup>> GetObservableConversations();
        IObservable<ImmutableList<Message>> GetObservableMessages(ConversationGroup group);
        IObservable<string> GetObservableTyping(ConversationGroup group);

        IObservable<ImmutableList<string>> GetObservableAllUsers();

        Task SendGlobalMessage(string message);
        Task SendChat(MessageRoute route, string content);
        Task SendTyping(MessageRoute route);
        Task MarkChatRead(int messageId, string currentUserName);

        Task<ConversationGroup> CreateGroup(string customName, ImmutableList<string> users);
        
    }
}
