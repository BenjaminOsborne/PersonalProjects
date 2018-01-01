using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ChatServiceLayer.Shared;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR.Client;

namespace ChatServiceLayer
{
    public interface IOutputLogger
    {
        void WriteLine();
        void WriteLine(string log);
    }

    public class WritterLogger : IOutputLogger
    {
        private readonly TextWriter _writer;

        public WritterLogger(TextWriter writer)
        {
            _writer = writer;
        }

        public void WriteLine() => _writer.WriteLine();
        public void WriteLine(string log) => _writer.WriteLine(log);
    }

    public class ChatClient : IDisposable
    {
        #region Fields

        private readonly string _url;
        private readonly IOutputLogger _logger;
        private readonly Func<string, IEnumerable<string>, bool> _fnGroupExistsWithUsers;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClient _httpClient;

        private readonly Subject<ConversationGroup> _conversationGroups = new Subject<ConversationGroup>();
        private readonly Subject<Message> _messages = new Subject<Message>();
        private readonly Subject<MessageRoute> _conversationTyping = new Subject<MessageRoute>();

        [CanBeNull]
        private string _token;

        private HubConnection _chatConnection;
        private IHubProxy _chatHub;
        private string _userName;

        #endregion

        public ChatClient(string url, IOutputLogger logger, Func<string, IEnumerable<string>, bool> fnGroupExistsWithUsers)
        {
            _url = url;
            _logger = logger;
            _fnGroupExistsWithUsers = fnGroupExistsWithUsers;

            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
            _httpClient = new HttpClient(handler);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _chatConnection?.Dispose();

            _conversationGroups.OnCompleted();
            _conversationGroups.Dispose();

            _messages.OnCompleted();
            _messages.Dispose();

            _conversationTyping.OnCompleted();
            _conversationTyping.Dispose();
        }

        public async Task InitialiseConnection(string username, string password)
        {
            var loginUrl = _url + "Account/Login";
            _logger.WriteLine($"Sending http GET to {loginUrl}");

            var loginGet = await _httpClient.GetAsync(loginUrl);
            var loginGetContent = await loginGet.Content.ReadAsStringAsync();

            var initialToken = _ParseRequestVerificationToken(loginGetContent);
            var authContent = $"{initialToken}&UserName={username}&Password={password}&RememberMe=false";

            _logger.WriteLine($"Sending http POST to {loginUrl}");

            var loginPost = await _httpClient.PostAsync(loginUrl, new StringContent(authContent, Encoding.UTF8, "application/x-www-form-urlencoded"));
            var loginPostContent = await loginPost.Content.ReadAsStringAsync();
            _token = _ParseRequestVerificationToken(loginPostContent);
        }

        public IObservable<ConversationGroup> GetObservableConversations() => _conversationGroups;

        public IObservable<MessageRoute> GetObservableTyping() => _conversationTyping;

        public IObservable<Message> GetObservableMessages() => _messages;
        
        public async Task RunChatHub(string username)
        {
            _userName = username;
            var chatHubName = "ChatHub";

            var onConnected = "onConnected";
            var onConnectedHistory = "onConnectedHistory";

            var onEcho = "onEcho";
            var onCreatedGroup = "onCreatedGroup";

            var onSendChatAll = "onSendChatAll";
            var onSendChat = "onSendChat";
            var onSendTyping = "onSendTyping";
            
            _logger.WriteLine("Begin Hub");

            _chatConnection = new HubConnection(_url) { CookieContainer = _cookieContainer }; //TODO: Investigate adding to hub connection: "Credentials = CredentialCache.DefaultCredentials" ?
            _chatConnection.Error += exception => _logger.WriteLine($"Error: {exception.GetType()}: {exception.Message}");

            _chatHub = _chatConnection.CreateHubProxy(chatHubName);

            _chatHub.On<string>(onConnected, _OnConnected);
            _chatHub.On<Shared.ChatHistories>(onConnectedHistory, _OnConnectedHistory);
            _chatHub.On<string>(onEcho, _OnEcho);
            _chatHub.On<Shared.ConversationGroup>(onCreatedGroup, _OnCreatedGroup);

            _chatHub.On<Shared.Message>(onSendChatAll, _MessageFromUser);
            _chatHub.On<Shared.Message>(onSendChat, _MessageFromUser);
            _chatHub.On<Shared.MessageRoute>(onSendTyping, _OnTyping);
            
            await _chatConnection.Start();
        }

        public async Task Echo() => await _chatHub.Invoke("echo");

        public async Task SendGlobalMessage(string message) => await _chatHub.Invoke("sendChatAll", message);

        public async Task SendChat(Shared.MessageSendInfo sendInfo) => await _chatHub.Invoke("sendChat", sendInfo);

        public async Task SendTyping(Shared.MessageRoute route) => await _chatHub.Invoke("sendTyping", route);

        public async Task MarkChatRead(MessageReadInfo info) => await _chatHub.Invoke("markChatRead", info);

        public async Task<Shared.ConversationGroup> CreateGroup(Shared.ConversationGroup group) => await _chatHub.Invoke<Shared.ConversationGroup>("createGroup", group);

        public async Task<bool> AccountLogout()
        {
            var token = _token;
            if (token.IsNullOrEmpty())
            {
                return false;
            }

            var encoding = "application/x-www-form-urlencoded";
            var content = token.IsNullOrEmpty() ? new StringContent("", Encoding.UTF8, encoding)
                                                : new StringContent(token, Encoding.UTF8, encoding);

            _logger.WriteLine($"Sending http POST to {_url}/Account/LogOff");
            var logOff = await _httpClient.PostAsync(_url + "Account/LogOff", content);
            return logOff.IsSuccessStatusCode;

            //var logOut = await _httpClient.PostAsync(_url + "Account/Logout", content);
        }

        public async Task TestEcho()
        {
            _logger.WriteLine("Begin SignalR Connection");

            using (var connection = new Connection($"{_url}echo"))
            {
                connection.CookieContainer = _cookieContainer;
                connection.Error += ex => _logger.WriteLine($"Error: {ex.GetType()}: {ex.Message}");
                connection.Received += data => _logger.WriteLine($"Received: {data}");

                await connection.Start();
                await connection.Send("Sending to AuthorizeEchoConnection");
            }
        }

        private async void _OnConnected(string sender)
        {
            _OnEcho(sender);
            await Echo(); //Invoke call back to notify all other users
        }

        private void _OnConnectedHistory(ChatHistories dto)
        {
            foreach (var history in dto.Histories)
            {
                var group = _MapGroup(history.ConversationGroup);
                if (group == null)
                {
                    continue;
                }

                _conversationGroups.OnNext(group);

                foreach (var msg in history.Messages)
                {
                    var message = _MapMessage(msg);
                    if (message == null)
                    {
                        continue;
                    }
                    _messages.OnNext(message);
                }
            }
        }

        private async void _OnEcho(string sender)
        {
            var users = new [] { _userName, sender }.Distinct().OrderBy(x => x).ToArray();
            var customName = ""; //Default to empty
            var exits = _fnGroupExistsWithUsers(customName, users);
            if (exits)
            {
                return;
            }
            var dto = new Shared.ConversationGroup { Id = null, Name = "", Users = users};
            await CreateGroup(dto);
        }

        private void _OnCreatedGroup(Shared.ConversationGroup dto)
        {
            var group = _MapGroup(dto);
            if (group == null)
            {
                return;
            }
            _conversationGroups.OnNext(group);
        }

        private void _MessageFromUser(Shared.Message dto)
        {
            var message = _MapMessage(dto);
            if (message == null)
            {
                return;
            }
            _conversationGroups.OnNext(message.Route.Group);
            _messages.OnNext(message);
        }

        private void _OnTyping(Shared.MessageRoute dto)
        {
            var route = _MapMessageRoute(dto);
            if (route == null)
            {
                return;
            }
            _conversationTyping.OnNext(route);
        }

        [CanBeNull]
        private static ConversationGroup _MapGroup(Shared.ConversationGroup dto)
        {
            if (dto.Id.HasValue == false)
            {
                return null;
            }
            return ConversationGroup.CreateFromExisting(dto.Id.Value, dto.Name, dto.Users);
        }

        [CanBeNull]
        private static Message _MapMessage(Shared.Message dto)
        {
            var route = _MapMessageRoute(dto.Route);
            if (route == null)
            {
                return null;
            }
            var readStates = dto.ReadStates.Select(x => new ReadState(x.User, x.HasRead)).ToImmutableList();
            return new Message(dto.Id, dto.MessageTime, route, dto.Content, readStates);
        }

        [CanBeNull]
        private static MessageRoute _MapMessageRoute(Shared.MessageRoute dtoRoute)
        {
            var group = _MapGroup(dtoRoute.Group);
            if (group == null)
            {
                return null;
            }
            return new MessageRoute(group, dtoRoute.Sender);
        }

        [CanBeNull]
        private string _ParseRequestVerificationToken(string content)
        {
            var startIndex = content.IndexOf("__RequestVerificationToken");
            if (startIndex < 0)
            {
                return null;
            }

            var endIndex = content.IndexOf("\" />", startIndex);
            var length = endIndex - startIndex;
            var substring = content.Substring(startIndex, length);
            var token = substring.Replace("\" type=\"hidden\" value=\"", "=");
            return token;
        }
    }
}
