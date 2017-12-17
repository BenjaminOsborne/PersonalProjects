using System;
using ConsoleAppServer;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

[assembly: OwinStartup(typeof(Program.Startup))]
namespace ConsoleAppServer
{
    class Program
    {
        static IDisposable SignalR;

        static void Main(string[] args)
        {
            string url = "http://127.0.0.1:8088";
            SignalR = WebApp.Start(url);

            Console.ReadKey();
        }

        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseCors(CorsOptions.AllowAll);
                app.MapSignalR();
            }
        }

        [HubName("MyHub")]
        public class MyHub : Hub
        {
            public void Send(string name, string message)
            {
                Clients.All.addMessage(name, message);
            }
        }
    }
}