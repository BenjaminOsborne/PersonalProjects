using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace CSharpClient
{
    class CommonClient
    {
        private readonly TextWriter _traceWriter;

        public CommonClient(TextWriter traceWriter)
        {
            _traceWriter = traceWriter;
        }

        public async Task RunAsync(string url)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();

            using (var httpClient = new HttpClient(handler))
            {
                var loginUrl = url + "Account/Login";

                _traceWriter.WriteLine("Sending http GET to {0}", loginUrl);

                var response = await httpClient.GetAsync(loginUrl);
                var content = await response.Content.ReadAsStringAsync();
                var requestVerificationToken = ParseRequestVerificationToken(content);
                content = requestVerificationToken + "&UserName=Ben&Password=Tester&RememberMe=false";

                _traceWriter.WriteLine("Sending http POST to {0}", loginUrl);

                response = await httpClient.PostAsync(loginUrl, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"));
                content = await response.Content.ReadAsStringAsync();
                requestVerificationToken = ParseRequestVerificationToken(content);

                await RunPersistentConnection(url, handler.CookieContainer);
                await RunHub(url, handler.CookieContainer);

                _traceWriter.WriteLine();
                _traceWriter.WriteLine("Sending http POST to {0}", url + "Account/LogOff");
                response = await httpClient.PostAsync(url + "Account/LogOff", CreateContent(requestVerificationToken));

                _traceWriter.WriteLine("Sending http POST to {0}", url + "Account/Logout");
                response = await httpClient.PostAsync(url + "Account/Logout", CreateContent(requestVerificationToken));
            }
        }

        private async Task RunPersistentConnection(string url, CookieContainer cookieContainer)
        {
            _traceWriter.WriteLine();
            _traceWriter.WriteLine("*** Persistent Connection ***");

            using (var connection = new Connection(url + "echo"))
            {
                connection.CookieContainer = cookieContainer;
                
                connection.Received += data =>
                {
                    _traceWriter.WriteLine("Received: " + data);
                };

                connection.Error += exception =>
                {
                    _traceWriter.WriteLine("Error: {0}: {1}" + exception.GetType(), exception.Message);
                };

                await connection.Start();
                await connection.Send("sending to AuthorizeEchoConnection");
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
        }

        private async Task RunHub(string url, CookieContainer cookieContainer)
        {
            _traceWriter.WriteLine();
            _traceWriter.WriteLine("*** Hub ***");

            using (var connection = new HubConnection(url))
            {
                connection.CookieContainer = cookieContainer;
                
                connection.Error += exception =>
                {
                    _traceWriter.WriteLine("Error: {0}: {1}" + exception.GetType(), exception.Message);
                };

                var authorizeEchoHub = connection.CreateHubProxy("AuthorizeEchoHub");

                authorizeEchoHub.On<string>("hubReceived", data =>
                {
                    _traceWriter.WriteLine("HubReceived: " + data);
                });

                var chatHub = connection.CreateHubProxy("ChatHub");
                chatHub.On<string>("hubReceived", data => _traceWriter.WriteLine("ChatHubReceived: " + data));
                chatHub.On<string, string>("addMessage", (s1, s2) => _traceWriter.WriteLine(s1 + ": " + s2));

                await connection.Start();
                await authorizeEchoHub.Invoke("echo", "sending to AuthorizeEchoHub");
                
                while (true)
                {
                    var text = Console.ReadLine();
                    await chatHub.Invoke("broadcast", "Ben", text);

                    await Task.Delay(TimeSpan.FromSeconds(0.1));
                }
            }
        }

        private string ParseRequestVerificationToken(string content)
        {
            var startIndex = content.IndexOf("__RequestVerificationToken");

            if (startIndex == -1)
            {
                return null;
            }

            content = content.Substring(startIndex, content.IndexOf("\" />", startIndex) - startIndex);
            content = content.Replace("\" type=\"hidden\" value=\"", "=");
            return content;
        }

        private StringContent CreateContent(string requestVerificationToken)
        {
            var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
            if (!string.IsNullOrEmpty(requestVerificationToken))
            {
                content = new StringContent(requestVerificationToken, Encoding.UTF8, "application/x-www-form-urlencoded");
            }

            return content;
        }
    }
}
