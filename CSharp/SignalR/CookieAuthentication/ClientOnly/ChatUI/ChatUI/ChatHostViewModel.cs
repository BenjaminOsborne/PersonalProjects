using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using ChatServiceLayer;

namespace ChatUI
{
    public class ChatHostViewModel : ViewModelBase
    {
        public ChatHostViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService)
        {
            Login = new LoginViewModel(chatService, userName =>
            {
                Users = new UsersViewModel(schedulerProvider, chatService, userName);
                OnPropertyChanged(nameof(Users));
            });
        }

        public LoginViewModel Login { get; }
        public UsersViewModel Users { get; private set; }
    }

    public class LoginViewModel : ViewModelBase
    {
        private string _userName;
        private string _password;
        private bool _isLoggingIn;

        public LoginViewModel(IChatService chatService, Action<string> onLogin)
        {
            Login = new RelayCommand(async () =>
            {
                IsLoggingIn = true;
                await chatService.Login(UserName, Password);
                onLogin(UserName);
                IsLoggingIn = false;
            });
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand Login { get; }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            private set => SetProperty(ref _isLoggingIn, value);
        }
    }

    public class UsersViewModel : ViewModelBase
    {
        private readonly IDesktopSchedulerProvider _schedulerProvider;
        private readonly IChatService _chatService;

        private readonly ObservableCollection<string> _users = new ObservableCollection<string>();
        private readonly Dictionary<string, ConversationViewModel> _conversations = new Dictionary<string, ConversationViewModel>();

        private string _selectedUser;
        private ConversationViewModel _currentConversation;

        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string currentUserName)
        {
            _schedulerProvider = schedulerProvider;
            _chatService = chatService;
            
            _chatService.GetObservableUsers()
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(users =>
            {
                _users.SetState(users);

                var missing = _users.Where(u => _conversations.ContainsKey(u) == false).ToImmutableList();
                missing.ForEach(u => _conversations[u] = new ConversationViewModel(schedulerProvider, chatService, u, currentUserName));
            });
        }

        public IEnumerable<string> Users => _users;

        public string SelectedUser
        {
            get => _selectedUser;
            set => SetPropertyWithAction(ref _selectedUser, value, _ =>
            {
                CurrentConversation = _conversations[_selectedUser];
            });
        }

        public ConversationViewModel CurrentConversation
        {
            get => _currentConversation;
            private set => SetProperty(ref _currentConversation, value);
        }
    }

    public class ConversationViewModel : ViewModelBase
    {
        private readonly ObservableCollection<ChatItem> _chatHistory = new ObservableCollection<ChatItem>();
        private string _currentChat;

        public ConversationViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string targetUser, string currentUserName)
        {
            TargetUser = targetUser;
            SendChat = new AsyncRelayCommand(async () =>
            {
                var chat = CurrentChat;
                CurrentChat = string.Empty; //Clear on send
                await chatService.SendChat(targetUser, chat);
            });

            chatService.GetObservableMessages(targetUser, currentUserName)
                .ObserveOn(schedulerProvider.Dispatcher).Subscribe(msgs =>
                {
                    var chats = msgs.Select(m =>
                    {
                        var fromThem = m.Sender == targetUser;
                        return new ChatItem(m.MessageId, m.Text, fromThem, !fromThem);
                    }).ToImmutableList();
                    _chatHistory.SetState(chats, (a,b) => a.Id == b.Id);
                });
        }

        public string TargetUser { get; }

        public string CurrentChat
        {
            get => _currentChat;
            set => SetProperty(ref _currentChat, value);
        }

        public ICommand SendChat { get; }

        public IEnumerable<ChatItem> ChatHistory => _chatHistory;
    }

    public class ChatItem
    {
        public ChatItem(Guid id, string message, bool fromThem, bool fromUs)
        {
            Id = id;
            Message = message;
            FromThem = fromThem;
            FromUs = fromUs;
        }

        public Guid Id { get; }
        public string Message { get; }
        public bool FromThem { get; }
        public bool FromUs { get; }
    }
}
