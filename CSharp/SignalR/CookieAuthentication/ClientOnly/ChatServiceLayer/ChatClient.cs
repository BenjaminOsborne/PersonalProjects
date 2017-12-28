using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
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

        private readonly Subject<string> _users = new Subject<string>();
        private readonly Subject<Message> _messages = new Subject<Message>();
        private readonly Subject<string> _otherUserTyping = new Subject<string>();

        [CanBeNull]
        private string _token;

        private HubConnection _chatConnection;
        private IHubProxy _chatHub;

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

        public IObservable<string> GetObservableUsers() => _users;

        public IObservable<string> GetObservableUserTyping() => _otherUserTyping;

        public IObservable<Message> GetObservableMessages() => _messages;
        
        public async Task RunChatHub(string username)
        {
            var chatHubName = "ChatHub";

            var onConnected = "onConnected";
            var onEcho = "onEcho";

            var onBroadcastAll = "onBroadcastAll";
            var onBroadcastSpecific = "onBroadcastSpecific";
            var onBroadcastCallBack = "onBroadcastCallBack";
            var onBroadcastTyping = "onBroadcastTyping";

            var echoMethod = "echo";

            _logger.WriteLine("Begin Hub");

            _chatConnection = new HubConnection(_url) { CookieContainer = _cookieContainer };
            _chatConnection.Error += exception => _logger.WriteLine($"Error: {exception.GetType()}: {exception.Message}");

            _chatHub = _chatConnection.CreateHubProxy(chatHubName);

            _chatHub.On<string>(onConnected, async s =>
            {
                _UserPing(s);
                await _chatHub.Invoke(echoMethod); //callback
            });
            _chatHub.On<string>(onEcho, _UserPing);

            _chatHub.On<Shared.Message>(onBroadcastAll, _MessageFromUser);
            _chatHub.On<Shared.Message>(onBroadcastSpecific, _MessageFromUser);
            _chatHub.On<Shared.Message>(onBroadcastCallBack, _MessageFromUser);
            _chatHub.On<string>(onBroadcastTyping, u => _otherUserTyping.OnNext(u));
            
            await _chatConnection.Start();
        }

        public async Task SendGlobalMessage(string message) => await _chatHub.Invoke("broadcastAll", message);

        public async Task SendChat(string receiver, string message) => await _chatHub.Invoke("broadcastSpecific", receiver, message);

        public async Task SendTyping(string receiver) => await _chatHub.Invoke("broadcastTyping", receiver);

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

        private void _UserPing(string user)
        {
            _users.OnNext(user);
        }

        private void _MessageFromUser(Shared.Message dto)
        {
            _users.OnNext(dto.Sender);
            _users.OnNext(dto.Receiver);

            _messages.OnNext(new Message(dto.MessageId, dto.MessageTime, dto.Sender, dto.Receiver, dto.Text));
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
