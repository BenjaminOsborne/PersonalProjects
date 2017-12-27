using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ChatUI.DesignerViewModels
{
    public class ChatHostMockViewModel
    {
        public LoginViewModel Login { get; } = new LoginViewModel(null, null);
        public UsersMockViewModel Users { get; } = new UsersMockViewModel();
    }

    public class UsersMockViewModel
    {
        public IEnumerable<string> Users { get; } = new[]
        {
            "Ben",
            "Terry",
            "Chatty McChatface"
        };

        public string SelectedUser { get; set; } = "Terry";

        public ConversationMockViewModel CurrentConversation { get; } = new ConversationMockViewModel();
    }

    public class ConversationMockViewModel
    {
        public string CurrentChat { get; set; } = "Some chat";

        public ICommand SendChat { get; }

        public IEnumerable<ChatItem> ChatHistory { get; } = new[]
        {
            _CreateChat("Hello!", true),
            _CreateChat("Hi!", false),
            _CreateChat("What a lovely message...\nPlease send another\nto\tme!", true),
        };

        private static ChatItem _CreateChat(string message, bool fromThem) => new ChatItem(Guid.NewGuid(), message, fromThem, !fromThem);
    }
}
