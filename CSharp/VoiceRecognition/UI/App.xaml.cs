using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SpeechClient _client;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            Current.MainWindow = window;
            
            var viewModel = new ScrollingTextViewModel();
            window.DataContext = viewModel;

            _client = new SpeechClient();
            _client
                .GetObservableEvents()
                .ObserveOnDispatcher()
                .Subscribe(item =>
                {
                    viewModel.Items.Add($"{item.Type}\t{item.Text}");
                });

            window.Show();
        }
    }

    public class ScrollingTextViewModel
    {
        public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();
    }
}
