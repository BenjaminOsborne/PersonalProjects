using System.Threading.Tasks;

namespace ChatServiceLayer
{
    public class ChatService : IChatService
    {
        public async Task<bool> Login(string userName, string password)
        {
            await Task.Delay(3000);

            return true;
        }
    }
}
