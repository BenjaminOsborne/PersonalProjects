using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
        void WriteLine(string log, params object[] logParams);
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
        public void WriteLine(string log, params object[] logParams) => _writer.WriteLine(log, logParams);
    }

    public class ChatClient : IDisposable
    {
        private readonly string _url;
        private readonly IOutputLogger _logger;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClient _httpClient;

        public ChatClient(string url, IOutputLogger logger)
        {
            _url = url;
            _logger = logger;

            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
            _httpClient = new HttpClient(handler);
        }

        public void Dispose() => _httpClient.Dispose();

        public async Task InitialiseConnection(string username, string password)
        {
            var loginUrl = _url + "Account/Login";
            _logger.WriteLine("Sending http GET to {0}", loginUrl);

            var loginGet = await _httpClient.GetAsync(loginUrl);
            var loginGetContent = await loginGet.Content.ReadAsStringAsync();

            var initialToken = _ParseRequestVerificationToken(loginGetContent);
            var authContent = $"{initialToken}&UserName={username}&Password={password}&RememberMe=false";

            _logger.WriteLine("Sending http POST to {0}", loginUrl);

            var loginPost = await _httpClient.PostAsync(loginUrl, new StringContent(authContent, Encoding.UTF8, "application/x-www-form-urlencoded"));
            var loginPostContent = await loginPost.Content.ReadAsStringAsync();

            await _RunPersistentConnection();
            await _RunHub(username);

            var token = _ParseRequestVerificationToken(loginPostContent);
            await _RunAccountLogout(token);
        }
        
        private async Task _RunPersistentConnection()
        {
            _logger.WriteLine("Begin SignalR Connection");

            using (var connection = new Connection(_url + "echo"))
            {
                connection.CookieContainer = _cookieContainer;
                connection.Error += ex => _logger.WriteLine($"Error: {ex.GetType()}: {ex.Message}");
                connection.Received += data => _logger.WriteLine($"Received: {data}");

                await connection.Start();
                await connection.Send("Sending to AuthorizeEchoConnection");
            }
        }

        private async Task _RunHub(string username)
        {
            _logger.WriteLine("Begin Hub");

            using (var connection = new HubConnection(_url))
            {
                connection.CookieContainer = _cookieContainer;
                
                connection.Error += exception => _logger.WriteLine("Error: {0}: {1}" + exception.GetType(), exception.Message);

                var authorizeEchoHub = connection.CreateHubProxy("AuthorizeEchoHub");

                authorizeEchoHub.On<string>("hubReceived", data => _logger.WriteLine("HubReceived: " + data));

                var chatHub = connection.CreateHubProxy("ChatHub");

                chatHub.On<string>("hubReceived", data => _logger.WriteLine($"ChatHubReceived: {data}"));
                chatHub.On<string, string>("addMessage", (s1, s2) => _logger.WriteLine($"{s1}: {s2}"));

                await connection.Start();
                await authorizeEchoHub.Invoke("echo", "sending to AuthorizeEchoHub");
                await chatHub.Invoke("echo", "sending to ChatHub");

                _ReadConsole().ToObservable()
                    .SubscribeOn(NewThreadScheduler.Default)
                    .Subscribe(async text =>
                {
                    await chatHub.Invoke("broadcast", username, text);
                });

                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(100));
                }
            }
        }

        private IEnumerable<string> _ReadConsole()
        {
            while (true)
                yield return Console.ReadLine();
        }

        private async Task _RunAccountLogout(string token)
        {
            var encoding = "application/x-www-form-urlencoded";
            var content = token.IsNullOrEmpty() ? new StringContent("", Encoding.UTF8, encoding)
                : new StringContent(token, Encoding.UTF8, encoding);

            _logger.WriteLine();
            _logger.WriteLine("Sending http POST to {0}", _url + "Account/LogOff");
            var logOff = await _httpClient.PostAsync(_url + "Account/LogOff", content);

            _logger.WriteLine("Sending http POST to {0}", _url + "Account/Logout");
            var logOut = await _httpClient.PostAsync(_url + "Account/Logout", content);
        }

        [CanBeNull]
        private string _ParseRequestVerificationToken(string content)
        {
            var startIndex = content.IndexOf("__RequestVerificationToken");
            if (startIndex < 0)
            {
                return null;
            }

            var substring = content.Substring(startIndex, content.IndexOf("\" />", startIndex) - startIndex);
            var token = substring.Replace("\" type=\"hidden\" value=\"", "=");
            return token;
        }
    }
}
