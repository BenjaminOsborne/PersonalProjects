using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public interface IChatService
    {
        Task<bool> Login(string userName, string password);
    }
}
