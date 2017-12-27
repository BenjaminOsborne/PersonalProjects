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
        private static readonly ConversationMockViewModel[] _users = new[]
        {
            _CreateUser("Ben", 0),
            _CreateUser("Terry", 2),
            _CreateUser("Chatty McChatface", 30)
        };

        private static ConversationMockViewModel _CreateUser(string name, int unread) => new ConversationMockViewModel(name, unread);

        public IEnumerable<ConversationMockViewModel> Users { get; } = _users;

        public ConversationMockViewModel SelectedUser { get; set; } = _users[1];
    }

    public class ConversationMockViewModel
    {
        public ConversationMockViewModel(string targetUser, int unread)
        {
            TargetUser = targetUser;
            Unread = unread;
        }

        public string TargetUser { get; }

        public int Unread { get; }

        public bool ShowUnread => Unread > 0;

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
