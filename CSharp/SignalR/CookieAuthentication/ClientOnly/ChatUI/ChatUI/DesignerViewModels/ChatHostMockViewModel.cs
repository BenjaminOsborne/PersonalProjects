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
        private static readonly UserViewModel[] _users = new[]
        {
            _CreateUser("Ben", 0),
            _CreateUser("Terry", 2),
            _CreateUser("Chatty McChatface", 30)
        };

        private static UserViewModel _CreateUser(string name, int unread) => new UserViewModel(name) { Unread = unread };

        public IEnumerable<UserViewModel> Users { get; } = _users;

        public UserViewModel SelectedUser { get; set; } = _users[1];

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
