using System;
using System.Collections.Generic;
using System.Windows.Input;
using ChatServiceLayer;

namespace ChatUI.DesignerViewModels
{
    public class ChatHostMockViewModel
    {
        public string Title { get; } = "Chat for Terry";
        public LoginViewModel Login { get; } = new LoginViewModel(null, null);
        public UsersMockViewModel Users { get; } = new UsersMockViewModel();
        public ChatsMockViewModel Chats { get; } = new ChatsMockViewModel();
    }

    public class UsersMockViewModel
    {
        public ICommand FlickVisible { get; }

        public string FlickDisplay { get; } = "+";

        public bool CreateConversation { get; } = true;

        public bool ShowExistingConversations { get; } = true;

        public IEnumerable<CheckUserViewModel> Users { get; } = new[]
        {
            _Create("Ben", true),
            _Create("Terry", false),
            _Create("Frank", true),
        };

        private static CheckUserViewModel _Create(string name, bool isChecked) => new CheckUserViewModel(name) { IsChecked = isChecked};

        public ICommand CreateGroup { get; }
    }

    public class ChatsMockViewModel
    {
        private static readonly ConversationMockViewModel[] _users = new[]
        {
            _CreateUser("Ben", 2),
            _CreateUser("Terry", 0),
            _CreateUser("Chatty McChatface", 30)
        };

        private static ConversationMockViewModel _CreateUser(string name, int unread) => new ConversationMockViewModel { ConversationTitle = name, Unread = unread };

        public IEnumerable<ConversationMockViewModel> Conversations { get; } = _users;

        public ConversationMockViewModel SelectedConversation { get; set; } = _users[1];
    }

    public class ConversationMockViewModel
    {
        public string ConversationTitle { get; set; }

        public ConversationGroup Conversation { get; set; }

        public int Unread { get; set; }

        public bool ShowUnread => Unread > 0;

        public string CurrentChat { get; set; } = "Some chat";

        public string ConversationTypingText => "Terry is typing...";

        public ICommand SendChat { get; }

        public IEnumerable<ChatItem> ChatHistory { get; } = new[]
        {
            _CreateChat("Ben", "Hello!", false),
            _CreateChat("Terry", "Hi!", true),
            _CreateChat("Ben", "What a lovely message...\nPlease send another\nto\tme!", false),
        };

        private static ChatItem _CreateChat(string sender, string message, bool fromUs) => new ChatItem(12, sender, message, fromUs);
    }
}
