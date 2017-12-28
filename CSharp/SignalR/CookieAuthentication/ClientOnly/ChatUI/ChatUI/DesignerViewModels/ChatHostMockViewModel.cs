using System;
using System.Collections.Generic;
using System.Windows.Input;
using ChatServiceLayer;

namespace ChatUI.DesignerViewModels
{
    public class ChatHostMockViewModel
    {
        public LoginViewModel Login { get; } = new LoginViewModel(null, null);
        public UsersMockViewModel Users { get; } = new UsersMockViewModel();
    }

    public class UsersMockViewModel
    {
        private static readonly ConversationMockViewModel[] _users = new[]
        {
            _CreateUser("Ben", 0),
            _CreateUser("Terry", 2),
            _CreateUser("Chatty McChatface", 30)
        };

        private static ConversationMockViewModel _CreateUser(string name, int unread) => new ConversationMockViewModel { ConversationTitle = name, Unread = unread };

        public IEnumerable<ConversationMockViewModel> Users { get; } = _users;

        public ConversationMockViewModel SelectedUser { get; set; } = _users[1];
    }

    public class ConversationMockViewModel
    {
        public string ConversationTitle { get; set; }

        public ConverationGroup Conversation { get; set; }

        public int Unread { get; set; }

        public bool ShowUnread => Unread > 0;

        public string CurrentChat { get; set; } = "Some chat";

        public string ConversationTypingText => "Terry is typing...";

        public ICommand SendChat { get; }

        public IEnumerable<ChatItem> ChatHistory { get; } = new[]
        {
            _CreateChat("Ben", "Hello!", true),
            _CreateChat("Terry", "Hi!", false),
            _CreateChat("Ben", "What a lovely message...\nPlease send another\nto\tme!", true),
        };

        private static ChatItem _CreateChat(string sender, string message, bool fromThem) => new ChatItem(Guid.NewGuid(), sender, message, fromThem, !fromThem);
    }
}
