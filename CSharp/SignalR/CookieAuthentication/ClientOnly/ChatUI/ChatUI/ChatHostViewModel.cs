using System;
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
        public ChatHostViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService)
        {
            Login = new LoginViewModel(chatService, currentUserName =>
            {
                Users = new UsersViewModel(schedulerProvider, chatService, currentUserName);
                OnPropertyChanged(nameof(Users));

                Chats = new ChatsViewModel(schedulerProvider, chatService, currentUserName);
                OnPropertyChanged(nameof(Chats));
            });
        }

        public LoginViewModel Login { get; }
        public UsersViewModel Users { get; private set; }
        public ChatsViewModel Chats { get; private set; }
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
        private readonly ObservableCollection<CheckUserViewModel> _users = new ObservableCollection<CheckUserViewModel>();
        private bool _isVisible;

        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string currentUserName)
        {
            FlickVisible = new RelayCommand(() => IsVisible = !IsVisible);

            CreateGroup = new AsyncRelayCommand(async () =>
            {
                var otherUsers = _users.Where(x => x.IsChecked).Select(x => x.User).ToImmutableList();
                var groupUsers = otherUsers.Add(currentUserName);
                var success = await chatService.CreateGroup(groupUsers);
                if (success == false)
                {
                    return;
                }

                IsVisible = false;
                foreach (var item in _users)
                {
                    item.IsChecked = false;
                }
            });

            chatService.GetObservableAllUsers().ObserveOn(schedulerProvider.Dispatcher).Subscribe(users =>
            {
                var filtered = users.Where(x => x != currentUserName)
                    .Select(x => new CheckUserViewModel(x)).ToImmutableList();
                _users.SetState(filtered, (a,b) => a.User == b.User);
            });
        }

        public bool IsVisible
        {
            get => _isVisible;
            private set => SetProperty(ref _isVisible, value);
        }

        public ICommand FlickVisible { get; }

        public IEnumerable<CheckUserViewModel> Users => _users;

        public ICommand CreateGroup { get; }
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
        private readonly ObservableCollection<ConversationViewModel> _users = new ObservableCollection<ConversationViewModel>();

        private ConversationViewModel _selectedUser;
        
        public ChatsViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string currentUserName)
        {
            _schedulerProvider = schedulerProvider;
            _chatService = chatService;
            _currentUserName = currentUserName;

            _chatService.GetObservableConversations()
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(users =>
            {
                var existing = _users.Select(x => x.Conversation).ToImmutableHashSet();
                var missing = users.Where(u => existing.Contains(u) == false).ToImmutableList();
                missing.ForEach(conversation =>
                {
                    var model = _CreateModelForConversation(conversation);
                    _users.Add(model);
                });
            });
        }

        public IEnumerable<ConversationViewModel> Users => _users;

        public ConversationViewModel SelectedUser
        {
            get => _selectedUser;
            set => SetPropertyWithAction(ref _selectedUser, value, _ =>
            {
                var selected = _selectedUser;

                var others = _users.Where(x => x != selected).ToImmutableList();
                others.ForEach(x => x.IsSelected = false);
                if (selected != null)
                {
                    selected.IsSelected = true;
                }
            });
        }

        private ConversationViewModel _CreateModelForConversation(ConverationGroup conversation)
        {
            var route = new MessageRoute(conversation, _currentUserName);

            var obsMessages = _chatService.GetObservableMessages(conversation)
                .ObserveOn(_schedulerProvider.Dispatcher).Publish();

            var model = new ConversationViewModel(
                obsMessages,
                c => _chatService.SendChat(route, c),
                () => _chatService.SendTyping(route),
                conversation, _currentUserName);

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
        private readonly ObservableCollection<ChatItem> _chatHistory = new ObservableCollection<ChatItem>();
        private ImmutableHashSet<Guid> _previousRead = ImmutableHashSet<Guid>.Empty;

        private int _unread;
        private string _currentChat;
        private bool _isSelected;
        private string _conversationTypingText;

        #endregion

        public ConversationViewModel(IObservable<ImmutableList<Message>> obsMessages, Func<string, Task> onSendChat, Action onTyping, ConverationGroup conversation, string currentUser)
        {
            _onTyping = onTyping;
            ConversationTitle = _GetTitle(conversation, currentUser);
            Conversation = conversation;
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
                    return new ChatItem(m.MessageId, sender, m.Content, fromCurrent: sender == currentUser);
                }).ToImmutableList();
                _chatHistory.SetState(chats, (a,b) => a.Id == b.Id);

                _AnalyseUnread();
            });
        }

        public string ConversationTitle { get; }

        public ConverationGroup Conversation { get; }

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

        private string _GetTitle(ConverationGroup conversation, string currentUser)
        {
            if (conversation.Users.Count == 1 && conversation.Users[0] == currentUser)
            {
                return currentUser;
            }

            var otherUsers = conversation.Users.Where(x => x != currentUser);
            return string.Join(", ", otherUsers);
        }

        private void _AnalyseUnread()
        {
            if (IsSelected)
            {
                _previousRead = _chatHistory.Select(x => x.Id).ToImmutableHashSet();
                Unread = 0;
            }
            else
            {
                Unread = _chatHistory.Count(x => _previousRead.Contains(x.Id) == false);
            }
        }
    }

    public class ChatItem
    {
        public ChatItem(Guid id, string sender, string message, bool fromCurrent)
        {
            Id = id;
            Sender = sender;
            Message = message;
            FromCurrent = fromCurrent;
        }

        public Guid Id { get; }
        public string Sender { get; }
        public string Message { get; }
        public bool FromCurrent { get; }
        public bool FromOther => !FromCurrent;
    }
}
