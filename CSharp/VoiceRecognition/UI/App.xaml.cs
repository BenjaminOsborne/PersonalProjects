using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using Google.Cloud.Speech.V1;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AzureSpeechClient _azureClient;
        private GoogleSpeechClient _googleClient;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            Current.MainWindow = window;
            
            var azureModel = new ItemsViewModel();
            var googleModel = new ItemsViewModel();
            window.DataContext = new ScrollingTextViewModel(azureModel, googleModel);

            _azureClient = new AzureSpeechClient();
            var azureSub = _azureClient
                .GetObservableEvents()
                .ObserveOnDispatcher()
                .Subscribe(item => azureModel.AddResult(item));

            _googleClient = await GoogleSpeechClient.StartRecording();
            var googleSub = _googleClient
                .GetObserveableEvents()
                .ObserveOnDispatcher()
                .Subscribe(item => googleModel.AddResult(item));

            window.Show();

            bool shouldExit = false;
            window.Closing += async (sender, args) =>
            {
                if (shouldExit)
                {
                    return;
                }

                azureSub.Dispose();
                googleSub.Dispose();

                _azureClient.Dispose();

                args.Cancel = true;
                await _googleClient.StopRecording();
                shouldExit = true;
                window.Close();
            };
        }
    }

    public class AzureResultItemViewModel : ResultItemViewModel
    {
        public AzureResultItemViewModel(string display, Brush brush, SpeechEvent result)
            : base(display, brush, result.Type == SpeechEventType.PartialResponse)
        {
            Result = result;
        }
        public SpeechEvent Result { get; }
    }

    public class GoogleResultItemViewModel : ResultItemViewModel
    {
        public GoogleResultItemViewModel(string display, Brush brush, bool isPartial, StreamingRecognizeResponse result)
            : base(display, brush, isPartial)
        {
            Result = result;
        }
        public StreamingRecognizeResponse Result { get; }
    }

    public abstract class ResultItemViewModel
    {
        protected ResultItemViewModel(string display, Brush brush, bool isPartial)
        {
            Display = display;
            Brush = brush;
            IsPartial = isPartial;
        }

        public string Display { get; }
        public Brush Brush { get; }
        public bool IsPartial { get; }
    }

    public class ScrollingTextViewModel
    {
        public ScrollingTextViewModel(ItemsViewModel azureModel, ItemsViewModel googleModel)
        {
            AzureModel = azureModel;
            GoogleModel = googleModel;
        }

        public IEnumerable<ResultItemViewModel> AzureItems => AzureModel.Items;
        public IEnumerable<ResultItemViewModel> GoogleItems => GoogleModel.Items;

        public ItemsViewModel AzureModel { get; }
        public ItemsViewModel GoogleModel { get; }

    }

    public class ItemsViewModel
    {
        private readonly ObservableCollection<ResultItemViewModel> _items = new ObservableCollection<ResultItemViewModel>();
        private Action _preAction;
        private Action _postAction;

        public IEnumerable<ResultItemViewModel> Items => _items;

        public void RegisterPreMessageUpdateAction(Action action) => _preAction = action;

        public void RegisterPostMessageUpdateAction(Action action) => _postAction = action;
        
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
                    _ReplacePartialOrAdd(item, Brushes.Green);
                    break;
                case SpeechEventType.End:
                    _AddItem(item, Brushes.Black);
                    break;
                case SpeechEventType.Error:
                    _AddItem(item, Brushes.Red, overrideText: $"<{item.Text}>");
                    break;
                case SpeechEventType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void _ReplacePartialOrAdd(SpeechEvent item, Brush brush)
        {
            if (_items.Count == 0 || _items.Last().IsPartial == false)
            {
                _AddItem(item, brush);
            }
            else
            {
                _items[_items.Count - 1] = new AzureResultItemViewModel(item.Text, brush, item);
            }
        }

        private void _AddItem(SpeechEvent item, Brush brush, string overrideText = null)
        {
            var viewModel = new AzureResultItemViewModel(overrideText ?? item.Text, brush, item);
            _preAction?.Invoke();
            _items.Add(viewModel);
            _postAction?.Invoke();
        }
    }
    
    public class ScrollingTextMockViewModel
    {
        public ScrollingTextMockViewModel()
        {
            var items = new[]
            {
                new AzureResultItemViewModel("Test text", Brushes.Green, null),
                new AzureResultItemViewModel("More", Brushes.Orange, null),
            };
            AzureItems = items;
            GoogleItems = items;
        }
        public IEnumerable<ResultItemViewModel> AzureItems { get; }
        public IEnumerable<ResultItemViewModel> GoogleItems { get; }
    }
}
