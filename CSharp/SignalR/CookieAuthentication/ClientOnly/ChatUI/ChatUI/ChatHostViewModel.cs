﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatServiceLayer;

namespace ChatUI
{
    public class ChatHostViewModel : ViewModelBase
    {
        private string _title;

        public ChatHostViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService)
        {
            _title = "Login";

            Login = new LoginViewModel(chatService, currentUserName =>
            {
                Title = $"Chat for {currentUserName}";

                Users = new UsersViewModel(schedulerProvider, chatService, _OnCreatedGroup, currentUserName);
                OnPropertyChanged(nameof(Users));

                Chats = new ChatsViewModel(schedulerProvider, chatService, currentUserName);
                OnPropertyChanged(nameof(Chats));
            });
        }

        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public LoginViewModel Login { get; }
        public UsersViewModel Users { get; private set; }
        public ChatsViewModel Chats { get; private set; }

        private void _OnCreatedGroup(ConversationGroup group)
        {
            var chats = Chats;
            var found = chats?.Conversations?.FirstOrDefault(x => x.Conversation == group);
            if (found != null)
            {
                chats.SelectedConversation = found;
            }
        }
    }

    public class LoginViewModel : ViewModelBase
    {
        #region Fields

        private readonly IChatService _chatService;
        private readonly Action<string> _onLogin;

        private bool _showLogin;
        private string _userName;
        private string _password;
        private bool _isLoggingIn;

        #endregion

        public LoginViewModel(IChatService chatService, Action<string> onLogin)
        {
            _chatService = chatService;
            _onLogin = onLogin;
            _showLogin = true;

            Login = new AsyncRelayCommand(_Login);
        }

        public bool ShowLogin
        {
            get => _showLogin;
            private set => SetProperty(ref _showLogin, value);
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

        private async Task _Login()
        {
            IsLoggingIn = true;

            var success = await _chatService.Login(UserName, Password);
            IsLoggingIn = false;

            if (success)
            {
                _onLogin(UserName);
                ShowLogin = false;
            }
        }
    }

    public class UsersViewModel : ViewModelBase
    {
        #region Fields

        private readonly IChatService _chatService;
        private readonly Action<ConversationGroup> _onCreatedGroup;
        private readonly string _currentUserName;
        private readonly ObservableCollection<CheckUserViewModel> _users = new ObservableCollection<CheckUserViewModel>();

        private bool _createConversation;
        private string _customName;

        #endregion
        
        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, Action<ConversationGroup> onCreatedGroup, string currentUserName)
        {
            _chatService = chatService;
            _onCreatedGroup = onCreatedGroup;
            _currentUserName = currentUserName;

            FlickVisible = new RelayCommand(() => CreateConversation = !CreateConversation);
            CreateGroup = new AsyncRelayCommand(_CreateGroup);

            _chatService.GetObservableAllUsers().ObserveOn(schedulerProvider.Dispatcher).Subscribe(_OnNextUsers);
        }

        public bool CreateConversation
        {
            get => _createConversation;
            private set => SetPropertyWithAction(ref _createConversation, value, _ =>
            {
                OnPropertyChangedExplicit(nameof(ShowExistingConversations));
                OnPropertyChangedExplicit(nameof(FlickDisplay));
            });
        }

        public bool ShowExistingConversations => !CreateConversation;

        public ICommand FlickVisible { get; }

        public string FlickDisplay => CreateConversation ? "-" : "+";

        public string CustomName
        {
            get => _customName;
            set => SetProperty(ref _customName, value);
        }

        public IEnumerable<CheckUserViewModel> Users => _users;

        public ICommand CreateGroup { get; }

        private async Task _CreateGroup()
        {
            var otherUsers = _users.Where(x => x.IsChecked).Select(x => x.User).ToImmutableHashSet();
            var groupUsers = otherUsers.Add(_currentUserName);
            var createdGroup = await _chatService.CreateGroup(CustomName, groupUsers.ToImmutableList());
            if (createdGroup == null)
            {
                return;
            }

            CustomName = ""; //Reset to empty after create
            CreateConversation = false; //Reset to hidden after create
            foreach (var item in _users)
            {
                item.IsChecked = false;
            }
            _onCreatedGroup(createdGroup);
        }

        private void _OnNextUsers(ImmutableList<string> users)
        {
            var filtered = users.Where(x => x != _currentUserName)
                .Select(x => new CheckUserViewModel(x)).ToImmutableList();
            _users.SetState(filtered, (a, b) => a.User == b.User);
        }
    }

    public class CheckUserViewModel : ViewModelBase
    {
        private bool _isChecked;

        public CheckUserViewModel(string user)
        {
            User = user;
        }

        public string User { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
    }

    public class ChatsViewModel : ViewModelBase
    {
        private readonly IDesktopSchedulerProvider _schedulerProvider;
        private readonly IChatService _chatService;
        private readonly string _currentUserName;
        private readonly ObservableCollection<ConversationViewModel> _conversations = new ObservableCollection<ConversationViewModel>();

        private ConversationViewModel _selectedConversation;
        
        public ChatsViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string currentUserName)
        {
            _schedulerProvider = schedulerProvider;
            _chatService = chatService;
            _currentUserName = currentUserName;

            _chatService.GetObservableConversations()
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(users =>
            {
                var existing = _conversations.Select(x => x.Conversation).ToImmutableHashSet();
                var missing = users.Where(u => existing.Contains(u) == false).ToImmutableList();

                if (missing.Any())
                {
                    foreach (var conversation in missing)
                    {
                        var model = _CreateModelForConversation(conversation);
                        _conversations.Add(model);
                    }

                    //Order by name
                    var ordered = _conversations
                        .OrderBy(x => x.ConverstionIsCurrentUserOnly == false)
                        .ThenBy(x => x.ConversationTitle)
                        .ToImmutableList();
                    _conversations.SetState(ordered);
                }
            });
        }

        public IEnumerable<ConversationViewModel> Conversations => _conversations;

        public ConversationViewModel SelectedConversation
        {
            get => _selectedConversation;
            set => SetPropertyWithAction(ref _selectedConversation, value, _ =>
            {
                var selected = _selectedConversation;

                var others = _conversations.Where(x => x != selected).ToImmutableList();
                others.ForEach(x => x.IsSelected = false);
                if (selected != null)
                {
                    selected.IsSelected = true;
                }
            });
        }

        private ConversationViewModel _CreateModelForConversation(ConversationGroup conversation)
        {
            var route = new MessageRoute(conversation, _currentUserName);

            var obsMessages = _chatService.GetObservableMessages(conversation)
                .ObserveOn(_schedulerProvider.Dispatcher).Publish();

            var model = new ConversationViewModel(
                obsMessages,
                onSendChat: c => _chatService.SendChat(route, c),
                onTyping: () => _chatService.SendTyping(route),
                fnMarkRead: id => _chatService.MarkChatRead(id, _currentUserName),
                conversation: conversation,
                currentUser: _currentUserName);

            //Handle typing text
            obsMessages.Subscribe(_ => model.ConversationTypingText = ""); //Clear whenever message comes in

            IDisposable runningClear = null;
            _chatService.GetObservableTyping(conversation)
                .Where(x => x != _currentUserName) //Filter out current user typing
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(sender =>
                {
                    runningClear?.Dispose(); //Dispose existing running clear

                    model.ConversationTypingText = $"{sender} is typing..."; //Set typing

                    var clearSpan = TimeSpan.FromSeconds(2);
                    runningClear = Observable.Timer(clearSpan).ObserveOn(_schedulerProvider.Dispatcher).Subscribe(__ =>
                    {
                        model.ConversationTypingText = ""; //Reset typing after span
                    });
                });

            obsMessages.Connect();

            return model;
        }
    }

    public class ConversationViewModel : ViewModelBase
    {
        #region Fields

        private readonly Action _onTyping;
        private readonly Action<int> _fnMarkRead;
        private readonly ObservableCollection<ChatItem> _chatHistory = new ObservableCollection<ChatItem>();

        private int _unread;
        private string _currentChat;
        private bool _isSelected;
        private string _conversationTypingText;

        #endregion

        public ConversationViewModel(IObservable<ImmutableList<Message>> obsMessages, Func<string, Task> onSendChat, Action onTyping, Action<int> fnMarkRead, ConversationGroup conversation, string currentUser)
        {
            _onTyping = onTyping;
            _fnMarkRead = fnMarkRead;

            Conversation = conversation;
            ConverstionIsCurrentUserOnly = conversation.Users.Count == 1 && conversation.Users[0] == currentUser;
            ConversationTitle = _GetTitle(conversation, ConverstionIsCurrentUserOnly, currentUser);
            
            SendChat = new AsyncRelayCommand(async () =>
            {
                var chat = CurrentChat;
                CurrentChat = string.Empty; //Clear on send
                await onSendChat(chat);
            });

            obsMessages.Subscribe(msgs =>
            {
                var chats = msgs.Select(m =>
                {
                    var sender = m.Route.Sender;
                    var hasRead = m.ReadStates.FirstOrDefault(s => s.User == currentUser)?.HasRead ?? false;
                    return new ChatItem(m.Id, sender, m.Content, fromCurrent: sender == currentUser, hasRead: hasRead);
                }).ToImmutableList();
                _chatHistory.SetState(chats);

                _AnalyseUnread();
            });
        }

        public bool ConverstionIsCurrentUserOnly { get; }

        public string ConversationTitle { get; }

        public ConversationGroup Conversation { get; }

        public int Unread
        {
            get => _unread;
            set => SetPropertyWithAction(ref _unread, value, _ => OnPropertyChangedExplicit(nameof(ShowUnread)));
        }

        public bool ShowUnread => Unread > 0;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetPropertyWithAction(ref _isSelected, value, _ => _AnalyseUnread());
        }

        public string CurrentChat
        {
            get => _currentChat;
            set => SetPropertyWithAction(ref _currentChat, value, _ => _onTyping());
        }

        public string ConversationTypingText
        {
            get => _conversationTypingText;
            set => SetProperty(ref _conversationTypingText, value);
        }

        public ICommand SendChat { get; }

        public IEnumerable<ChatItem> ChatHistory => _chatHistory;

        private static string _GetTitle(ConversationGroup conversation, bool converstionIsCurretUserOnly, string currentUser)
        {
            if (conversation.Name.IsNullOrEmpty() == false)
            {
                return conversation.Name;
            }

            if (converstionIsCurretUserOnly)
            {
                return $"{currentUser} (you)";
            }

            var otherUsers = conversation.Users.Where(x => x != currentUser);
            return string.Join(", ", otherUsers);
        }
        
        private void _AnalyseUnread()
        {
            if (IsSelected)
            {
                var unread = _chatHistory.Where(x => x.HasRead == false).ToImmutableList();
                foreach (var item in unread)
                {
                    _fnMarkRead(item.Id);
                }
                Unread = 0;
            }
            else
            {
                Unread = _chatHistory.Count(x => x.HasRead == false);
            }
        }
    }

    public class ChatItem
    {
        public ChatItem(int id, string sender, string message, bool fromCurrent, bool hasRead)
        {
            Id = id;
            Sender = sender;
            Message = message;
            FromCurrent = fromCurrent;
            HasRead = hasRead;
        }

        public int Id { get; }
        public string Sender { get; }
        public string Message { get; }
        public bool FromCurrent { get; }
        public bool FromOther => !FromCurrent;
        public bool HasRead { get; }
    }
}
