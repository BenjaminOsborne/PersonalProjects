using System.Collections.Generic;

namespace ChatUI.DesignerViewModels
{
    public class ChatHostMockViewModel
    {
        public LoginViewModel Login { get; } = new LoginViewModel(null);
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
    }
}
