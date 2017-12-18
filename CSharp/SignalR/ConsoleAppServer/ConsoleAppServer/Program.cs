using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppServer;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.Cookies;
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

            while (true) //Message clients on loop
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                context.Clients.All.addMessage("Test Name", "Test Message");

                Thread.Sleep(5000);
            }

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

        [HubName("MyHubName")]
        public class MyHub : Hub
        {
            public void Send(string name, string message)
            {
                Clients.All.addMessage(name, message);
            }
        }
    }
}