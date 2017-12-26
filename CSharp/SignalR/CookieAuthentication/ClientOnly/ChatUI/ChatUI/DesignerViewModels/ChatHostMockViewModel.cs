using System.Collections.Generic;
using System.Windows.Input;

namespace ChatUI.DesignerViewModels
{
    public class ChatHostMockViewModel
    {
        public LoginViewModel Login { get; } = new LoginViewModel(null, null);
        public UsersMockViewModel Users { get; } = new UsersMockViewModel();
    }

    public class UsersMockViewModel
    {
        public IEnumerable<string> Users { get; } = new[]
        {
            "Ben",
            "Terry",
            "Chatty McChatface"
        };

        public string SelectedUser { get; set; } = "Terry";

        public string CurrentChat { get; set; }

        public ICommand SendChat { get; }

        public string ChatHistory { get; }
    }
}
