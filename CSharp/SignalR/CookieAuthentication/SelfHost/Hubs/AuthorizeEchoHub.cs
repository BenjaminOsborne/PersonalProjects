using System;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Common.Hubs
{
    [Authorize]
    public class AuthorizeEchoHub : Hub
    {
        public override Task OnConnected()
        {
            return Clients.Caller.hubReceived($"{nameof(AuthorizeEchoHub)} Welcome {Context.User.Identity.Name}!");
        }

        public void Echo(string value)
        {
            Clients.Caller.hubReceived(value);
        }
    }

    [Authorize]
    public class ChatHub : Hub
    {
        public override Task OnConnected() => Clients.All.onConnected(_CurrentUser());

        public void Echo() => Clients.All.onEcho(_CurrentUser());

        public void BroadcastAll(Guid messageId, string message) => Clients.All.onBroadcastAll(messageId, _CurrentUser(), message);

        public void BroadcastSpecific(Guid messageId, string targetUserId, string message)
        {
            var currentUser = _CurrentUser();

            Clients.User(targetUserId).onBroadcastSpecific(messageId, currentUser, targetUserId, message);
            Clients.User(currentUser).onBroadcastCallBack(messageId, currentUser, targetUserId, message);
        }

        private string _CurrentUser() => Context.User.Identity.Name;
    }
}
