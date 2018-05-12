using System;
using System.Collections.Generic;
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
            var sub = _client
                .GetObservableEvents()
                .ObserveOnDispatcher()
                .Subscribe(item =>
                {
                    viewModel.AddItem($"{item.Type}\t{item.Text}");
                });

            window.Show();
            window.Closing += (sender, args) =>
            {
                sub.Dispose();
                _client.Dispose();
            };
        }
    }

    public class ScrollingTextViewModel
    {
        private readonly ObservableCollection<string> _items = new ObservableCollection<string>();
        private Action _preAction;
        private Action _postAction;

        public IEnumerable<string> Items => _items;

        public void AddItem(string item)
        {
            _preAction?.Invoke();
            _items.Add(item);
            _postAction?.Invoke();
        }

        public void RegisterPreMessageUpdateAction(Action action) => _preAction = action;

        public void RegisterPostMessageUpdateAction(Action action) => _postAction = action;
    }

    public class ScrollingTextMockViewModel
    {
        public ScrollingTextMockViewModel()
        {
            Items = new[]
            {
                "Test text",
                "More"
            };
        }
        public IEnumerable<string> Items { get; }
    }
}
