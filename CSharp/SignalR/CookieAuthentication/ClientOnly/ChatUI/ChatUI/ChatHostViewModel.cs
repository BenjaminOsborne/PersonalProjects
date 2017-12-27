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
        private readonly ObservableCollection<UserViewModel> _users = new ObservableCollection<UserViewModel>();
        private readonly Dictionary<string, ConversationViewModel> _conversations = new Dictionary<string, ConversationViewModel>();

        private UserViewModel _selectedUser;
        private ConversationViewModel _currentConversation;

        public UsersViewModel(IDesktopSchedulerProvider schedulerProvider, IChatService chatService, string currentUserName)
        {
            chatService.GetObservableUsers()
                .ObserveOn(schedulerProvider.Dispatcher).Subscribe(users =>
            {
                var userModels = users.Select(x => new UserViewModel(x)).ToImmutableList();
                _users.SetState(userModels, (a,b) => a.UserName == b.UserName);

                var missing = userModels.Where(u => _conversations.ContainsKey(u.UserName) == false).ToImmutableList();
                missing.ForEach(model =>
                {
                    var targetUser = model.UserName;
                    var obsMessages = chatService.GetObservableMessages(targetUser, currentUserName)
                        .ObserveOn(schedulerProvider.Dispatcher).Publish();

                    _conversations[targetUser] = new ConversationViewModel(
                        obsMessages,
                        c => chatService.SendChat(targetUser, c),
                        targetUser);

                    obsMessages.Subscribe(m =>
                    {
                        model.Unread = m.Count;
                    });

                    obsMessages.Connect();
                });
            });
        }

        public IEnumerable<UserViewModel> Users => _users;

        public UserViewModel SelectedUser
        {
            get => _selectedUser;
            set => SetPropertyWithAction(ref _selectedUser, value, _ =>
            {
                var user = _selectedUser?.UserName;
                CurrentConversation = user != null ? _conversations[user] : null;
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

        public ConversationViewModel(IObservable<ImmutableList<Message>> obsMessages, Func<string, Task> onSendChat, string targetUser)
        {
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

    public class UserViewModel : ViewModelBase
    {
        private int _unread;

        public UserViewModel(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; }

        public int Unread
        {
            get => _unread;
            set => SetPropertyWithAction(ref _unread, value, _ => OnPropertyChangedExplicit(nameof(ShowUnread)));
        }

        public bool ShowUnread => Unread > 0;
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
