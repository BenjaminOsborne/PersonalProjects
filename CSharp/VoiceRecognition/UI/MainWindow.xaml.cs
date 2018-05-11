using System.Windows;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpeechClient _client;

        public MainWindow()
        {
            InitializeComponent();

            _client = new SpeechClient();
        }
    }
}
