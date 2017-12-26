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
        private readonly string _userName;

        private readonly ObservableCollection<string> _users = new ObservableCollection<string>();

        private string _selectedUser;
        private string _currentChat;
        private string _chatHistory;
        private IDisposable _obsMessages;

        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string userName)
        {
            _schedulerProvider = schedulerProvider;
            _chatService = chatService;
            _userName = userName;

            _chatService.GetObservableUsers()
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(u =>
            {
                _users.SetState(u);
            });

            SendChat = new AsyncRelayCommand(async () =>
            {
                var chat = CurrentChat;
                CurrentChat = string.Empty; //Clear on send
                await chatService.SendChat(SelectedUser, chat);
            });
        }

        public IEnumerable<string> Users => _users;

        public string SelectedUser
        {
            get => _selectedUser;
            set => SetPropertyWithAction(ref _selectedUser, value, _ => _ObserveMessages());
        }

        public string CurrentChat
        {
            get => _currentChat;
            set => SetProperty(ref _currentChat, value);
        }

        public ICommand SendChat { get; }

        public string ChatHistory
        {
            get => _chatHistory;
            private set => SetProperty(ref _chatHistory, value);
        }

        private void _ObserveMessages()
        {
            _obsMessages?.Dispose();
            ChatHistory = "";

            var sender = SelectedUser;
            if (sender == null)
            {
                return;
            }

            _obsMessages = _chatService.GetObservableMessages(sender, _userName)
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(m =>
                {
                    ChatHistory = string.Join("\n", m.Select(x => $"{x.Sender}: {x.Text}"));
                });
        }
    }

    public class ConversationViewModel : ViewModelBase
    {
        
    }
}
