using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using ChatServiceLayer;

namespace ChatUI
{
    public class ChatHostViewModel : ViewModelBase
    {
        public ChatHostViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService)
        {
            Login = new LoginViewModel(chatService);
            Users = new UsersViewModel(schedulerProvider, chatService);
        }

        public LoginViewModel Login { get; }
        public UsersViewModel Users { get; }
    }

    public class LoginViewModel : ViewModelBase
    {
        private string _userName;
        private string _password;
        private bool _isLoggingIn;

        public LoginViewModel(IChatService chatService)
        {
            Login = new RelayCommand(async () =>
            {
                IsLoggingIn = true;
                await chatService.Login(UserName, Password);
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
        private readonly ObservableCollection<string> _users = new ObservableCollection<string>();

        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService)
        {
            chatService.GetObservableUsers().ObserveOn(schedulerProvider.Dispatcher).Subscribe(u =>
            {
                _users.Clear();
                u.ForEach(_users.Add);
            });
        }

        public IEnumerable<string> Users => _users;
    }

    public class ConversationViewModel : ViewModelBase
    {
        
    }
}
