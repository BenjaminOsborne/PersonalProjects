using System.Windows;
using ChatServiceLayer;

namespace ChatUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var chatService = new ChatService();

            var window = new ChatHostWindow();
            window.DataContext = new ChatHostViewModel(chatService);

            Current.MainWindow = window;
            window.Show();
        }
    }
}
