using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public interface IChatService
    {
        Task<bool> Login(string username, string password);
        IObservable<ImmutableList<User>> GetObservableUsers();
    }
}
