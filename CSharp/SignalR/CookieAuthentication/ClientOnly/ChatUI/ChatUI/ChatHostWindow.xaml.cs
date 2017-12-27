using System.Windows;
using System.Windows.Input;

namespace ChatUI
{
    /// <summary>
    /// Interaction logic for ChatHostWindow.xaml
    /// </summary>
    public partial class ChatHostWindow : Window
    {
        public ChatHostWindow()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                Password.PasswordChanged += (sender, args) =>
                {
                    var pw = Password.Password;
                    var login = (DataContext as ChatHostViewModel)?.Login;
                    if (login != null)
                    {
                        login.Password = pw;
                    }
                };

                CurrentChat.PreviewKeyDown += (sender, args) =>
                {
                    if (args.Key == Key.Return && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) == false)
                    {
                        SendChat.Command.Execute(null);
                        args.Handled = true;
                    }
                };
            };
        }
    }
}
