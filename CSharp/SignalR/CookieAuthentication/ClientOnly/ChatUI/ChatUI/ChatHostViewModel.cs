﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using ChatServiceLayer;

namespace ChatUI
{
    public class ChatHostViewModel : ViewModelBase
    {
        public ChatHostViewModel(IChatService chatService)
        {
            Login = new LoginViewModel(chatService);
        }

        public LoginViewModel Login { get; }
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
        private ObservableCollection<User> _users = new ObservableCollection<User>();

        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService)
        {
            chatService.GetObservableUsers().ObserveOn(schedulerProvider.Dispatcher).Subscribe(u =>
            {

            });
        }

        public IEnumerable<User> Users => _users;
    }

    public class ConversationViewModel : ViewModelBase
    {
        
    }
}