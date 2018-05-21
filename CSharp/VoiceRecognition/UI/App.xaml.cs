using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AzureSpeechClient _client;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            Current.MainWindow = window;
            
            var viewModel = new ScrollingTextViewModel();
            window.DataContext = viewModel;

            _client = new AzureSpeechClient();
            var sub = _client
                .GetObservableEvents()
                .ObserveOnDispatcher()
                .Subscribe(item => viewModel.AddResult(item));

            window.Show();
            window.Closing += (sender, args) =>
            {
                sub.Dispose();
                _client.Dispose();
            };
        }
    }

    public class ResultItemViewModel
    {
        public ResultItemViewModel(SpeechEvent result, string display, Brush brush)
        {
            Result = result;
            Display = display;
            Brush = brush;
        }

        public SpeechEvent Result { get; }
        public string Display { get; }
        public Brush Brush { get; }
    }

    public class ScrollingTextViewModel
    {
        private readonly ObservableCollection<ResultItemViewModel> _items = new ObservableCollection<ResultItemViewModel>();
        private Action _preAction;
        private Action _postAction;

        public IEnumerable<ResultItemViewModel> Items => _items;

        public void AddResult(SpeechEvent item)
        {
            switch (item.Type)
            {
                case SpeechEventType.Begin:
                    _AddItem(item, Brushes.Blue);
                    break;
                case SpeechEventType.PartialResponse:
                    _ReplacePartialOrAdd(item, Brushes.Orange);
                    break;
                case SpeechEventType.DictationResponse:

                    if (string.IsNullOrEmpty(item.Text))
                        _AddItem(item, Brushes.Red, overrideText: $"<{item.Result.RecognitionStatus}>");
                    else
                        _ReplacePartialOrAdd(item, Brushes.Green);
                    break;
                case SpeechEventType.End:
                    _AddItem(item, Brushes.Black);
                    break;
                case SpeechEventType.Error:
                    _AddItem(item, Brushes.Red);
                    break;
                case SpeechEventType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RegisterPreMessageUpdateAction(Action action) => _preAction = action;

        public void RegisterPostMessageUpdateAction(Action action) => _postAction = action;

        private void _ReplacePartialOrAdd(SpeechEvent item, SolidColorBrush brush)
        {
            if (_items.Count == 0 || _items.Last().Result.Type != SpeechEventType.PartialResponse)
            {
                _AddItem(item, brush);
            }
            else
            {
                _items[_items.Count - 1] = new ResultItemViewModel(item, item.Text, brush);
            }
        }

        private void _AddItem(SpeechEvent item, Brush brush, string overrideText = null)
        {
            _preAction?.Invoke();
            _items.Add(new ResultItemViewModel(item, overrideText ?? item.Text, brush));
            _postAction?.Invoke();
        }
    }

    public class ScrollingTextMockViewModel
    {
        public ScrollingTextMockViewModel()
        {
            Items = new[]
            {
                new ResultItemViewModel(null, "Test text", Brushes.Green),
                new ResultItemViewModel(null, "More", Brushes.Orange),
            };
        }
        public IEnumerable<ResultItemViewModel> Items { get; }
    }
}
