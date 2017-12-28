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
        private readonly string _currentUserName;
        private readonly ObservableCollection<ConversationViewModel> _users = new ObservableCollection<ConversationViewModel>();

        private ConversationViewModel _selectedUser;
        
        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string currentUserName)
        {
            _schedulerProvider = schedulerProvider;
            _chatService = chatService;
            _currentUserName = currentUserName;

            _chatService.GetObservableUsers()
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(users =>
            {
                var existing = _users.Select(x => x.TargetUser).ToImmutableHashSet();
                var missing = users.Where(u => existing.Contains(u) == false).ToImmutableList();
                missing.ForEach(targetUser =>
                {
                    var model = _CreateModelForTargetUser(targetUser);
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

        private ConversationViewModel _CreateModelForTargetUser(string targetUser)
        {
            var obsMessages = _chatService.GetObservableMessages(targetUser, _currentUserName)
                .ObserveOn(_schedulerProvider.Dispatcher).Publish();

            var model = new ConversationViewModel(
                obsMessages,
                c => _chatService.SendChat(targetUser, c),
                () => _chatService.SendTyping(targetUser),
                targetUser);

            //Handle typing text
            obsMessages.Subscribe(_ => model.TargetUserTypingText = ""); //Clear whenever message comes in

            IDisposable runningClear = null;
            _chatService.GetObservableTyping(targetUser)
                .ObserveOn(_schedulerProvider.Dispatcher).Subscribe(_ =>
                {
                    runningClear?.Dispose(); //Dispose existing running clear

                    model.TargetUserTypingText = $"{targetUser} is typing..."; //Set typing

                    var clearSpan = TimeSpan.FromSeconds(2);
                    runningClear = Observable.Timer(clearSpan).ObserveOn(_schedulerProvider.Dispatcher).Subscribe(__ =>
                    {
                        model.TargetUserTypingText = ""; //Reset typing after span
                    });
                });

            obsMessages.Connect();

            return model;
        }
    }

    public class ConversationViewModel : ViewModelBase
    {
        private readonly Action _onTyping;
        private readonly ObservableCollection<ChatItem> _chatHistory = new ObservableCollection<ChatItem>();
        private ImmutableHashSet<Guid> _previousRead = ImmutableHashSet<Guid>.Empty;

        private int _unread;
        private string _currentChat;
        private bool _isSelected;
        private string _targetUserTypingText;

        public ConversationViewModel(IObservable<ImmutableList<Message>> obsMessages, Func<string, Task> onSendChat, Action onTyping, string targetUser)
        {
            _onTyping = onTyping;
            TargetUser = targetUser;
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
                    var fromThem = m.Sender == targetUser;
                    return new ChatItem(m.MessageId, m.Sender, m.Text, fromThem, !fromThem);
                }).ToImmutableList();
                _chatHistory.SetState(chats, (a,b) => a.Id == b.Id);

                _AnalyseUnread();
            });
        }

        public string TargetUser { get; }

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

        public string TargetUserTypingText
        {
            get => _targetUserTypingText;
            set => SetProperty(ref _targetUserTypingText, value);
        }

    public ICommand SendChat { get; }

        public IEnumerable<ChatItem> ChatHistory => _chatHistory;
        
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
        public ChatItem(Guid id, string sender, string message, bool fromThem, bool fromUs)
        {
            Id = id;
            Sender = sender;
            Message = message;
            FromThem = fromThem;
            FromUs = fromUs;
        }

        public Guid Id { get; }
        public string Sender { get; }
        public string Message { get; }
        public bool FromThem { get; }
        public bool FromUs { get; }
    }
}
