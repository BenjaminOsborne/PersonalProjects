using System;
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
        private readonly string _url;
        private readonly IOutputLogger _logger;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClient _httpClient;

        private readonly Subject<ConverationGroup> _users = new Subject<ConverationGroup>();
        private readonly Subject<Message> _messages = new Subject<Message>();
        private readonly Subject<MessageRoute> _otherUserTyping = new Subject<MessageRoute>();

        [CanBeNull]
        private string _token;

        private HubConnection _chatConnection;
        private IHubProxy _chatHub;
        private string _userName;

        public ChatClient(string url, IOutputLogger logger)
        {
            _url = url;
            _logger = logger;

            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
            _httpClient = new HttpClient(handler);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _chatConnection?.Dispose();

            _users.OnCompleted();
            _users.Dispose();

            _messages.OnCompleted();
            _messages.Dispose();
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

        public IObservable<ConverationGroup> GetObservableUsers() => _users;

        public IObservable<MessageRoute> GetObservableUserTyping() => _otherUserTyping;

        public IObservable<Message> GetObservableMessages() => _messages;
        
        public async Task RunChatHub(string username)
        {
            _userName = username;
            var chatHubName = "ChatHub";
            var onConnected = "onConnected";
            var onEcho = "onEcho";
            var onEchoGroup = "onEchoGroup";

            var onBroadcastAll = "onBroadcastAll";
            var onBroadcastSpecific = "onBroadcastSpecific";
            var onBroadcastTyping = "onBroadcastTyping";
            
            _logger.WriteLine("Begin Hub");

            _chatConnection = new HubConnection(_url) { CookieContainer = _cookieContainer };
            _chatConnection.Error += exception => _logger.WriteLine($"Error: {exception.GetType()}: {exception.Message}");

            _chatHub = _chatConnection.CreateHubProxy(chatHubName);

            _chatHub.On<string>(onConnected, _OnConnected);
            _chatHub.On<string>(onEcho, _UserPing);
            _chatHub.On<Shared.ConversationGroup>(onEchoGroup, _GroupPing);

            _chatHub.On<Shared.Message>(onBroadcastAll, _MessageFromUser);
            _chatHub.On<Shared.Message>(onBroadcastSpecific, _MessageFromUser);
            _chatHub.On<Shared.MessageRoute>(onBroadcastTyping, _OnTyping);
            
            await _chatConnection.Start();
        }
        
        public async Task Echo() => await _chatHub.Invoke("echo");

        public async Task SendGlobalMessage(string message) => await _chatHub.Invoke("broadcastAll", message);

        public async Task SendChat(Shared.MessageSendInfo sendInfo) => await _chatHub.Invoke("broadcastSpecific", sendInfo);

        public async Task SendTyping(Shared.MessageRoute route) => await _chatHub.Invoke("broadcastTyping", route);

        public async Task CreateGroup(ConverationGroup group) => await _chatHub.Invoke("echoGroup", group);

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
            _UserPing(sender);
            await Echo(); //Invoke call back to notify all other users
        }

        private void _UserPing(string sender)
        {
            var group = ConverationGroup.Create(new [] { _userName, sender });
            _users.OnNext(group);
        }

        private void _GroupPing(ConversationGroup dto)
        {
            var group = ConverationGroup.Create(dto.Users);
            _users.OnNext(group);
        }

        private void _MessageFromUser(Shared.Message dto)
        {
            var dtoRoute = dto.Route;
            var route = _CreateMessageRoute(dtoRoute);

            _users.OnNext(route.Group);

            var message = new Message(dto.MessageId, dto.MessageTime, route, dto.Content);
            _messages.OnNext(message);
        }

        private void _OnTyping(Shared.MessageRoute dto)
        {
            var route = _CreateMessageRoute(dto);
            _otherUserTyping.OnNext(route);
        }

        private static MessageRoute _CreateMessageRoute(Shared.MessageRoute dtoRoute)
        {
            var dtoGroup = dtoRoute.Group;
            var group = ConverationGroup.Create(dtoGroup.Users);
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
