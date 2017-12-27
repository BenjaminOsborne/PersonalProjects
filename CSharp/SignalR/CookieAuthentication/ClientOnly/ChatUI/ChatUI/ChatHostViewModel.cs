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
        private readonly ObservableCollection<ConversationViewModel> _users = new ObservableCollection<ConversationViewModel>();

        private ConversationViewModel _selectedUser;

        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string currentUserName)
        {
            chatService.GetObservableUsers()
                .ObserveOn(schedulerProvider.Dispatcher).Subscribe(users =>
            {
                var existing = _users.Select(x => x.TargetUser).ToImmutableHashSet();
                var missing = users.Where(u => existing.Contains(u) == false).ToImmutableList();
                missing.ForEach(targetUser =>
                {
                    var obsMessages = chatService.GetObservableMessages(targetUser, currentUserName)
                        .ObserveOn(schedulerProvider.Dispatcher).Publish();

                    var model = new ConversationViewModel(
                        obsMessages,
                        c => chatService.SendChat(targetUser, c),
                        () => chatService.SendTyping(targetUser),
                        targetUser);
                    _users.Add(model);

                    var span = TimeSpan.FromSeconds(2);
                    var obsMessageTicks = obsMessages.Select(_ => DateTime.Now).StartWith(DateTime.Now);
                    var obsTyping = chatService.GetObservableTyping(targetUser).Select(x => DateTime.Now);
                    var obsTimer = Observable.Interval(span).Select(_ => Unit.Instance).StartWith(Unit.Instance);

                    obsMessageTicks.CombineLatest(obsTyping, obsTimer, Tuple.Create)
                        .ObserveOn(schedulerProvider.Dispatcher)
                        .Subscribe(tup =>
                        {
                            var sendTime = tup.Item1;
                            var typingTime = tup.Item2;

                            var sentAfterType = sendTime > typingTime;
                            var isTypingInRange = typingTime > DateTime.Now.Add(-span);

                            if (sentAfterType == false && isTypingInRange)
                            {
                                model.TargetUserTypingText = $"{targetUser} is typing...";
                            }
                            else
                            {
                                model.TargetUserTypingText = "";
                            }
                        });

                    obsMessages.Connect();
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
                    return new ChatItem(m.MessageId, m.Text, fromThem, !fromThem);
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
