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
            
            var azureModel = new AzureItemsViewModel();
            var googleModel = new GoogleItemsViewModel();
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
        public ScrollingTextViewModel(AzureItemsViewModel azureModel, GoogleItemsViewModel googleModel)
        {
            AzureModel = azureModel;
            GoogleModel = googleModel;
        }

        public IEnumerable<ResultItemViewModel> AzureItems => AzureModel.Items;
        public IEnumerable<ResultItemViewModel> GoogleItems => GoogleModel.Items;

        public AzureItemsViewModel AzureModel { get; }
        public GoogleItemsViewModel GoogleModel { get; }

    }

    public abstract class ItemsViewModelBase
    {
        protected readonly ObservableCollection<ResultItemViewModel> _items = new ObservableCollection<ResultItemViewModel>();
        private Action _preAction;
        private Action _postAction;

        public IEnumerable<ResultItemViewModel> Items => _items;

        public void RegisterPreMessageUpdateAction(Action action) => _preAction = action;

        public void RegisterPostMessageUpdateAction(Action action) => _postAction = action;

        protected void _AddItem(ResultItemViewModel viewModel)
        {
            _preAction?.Invoke();
            _items.Add(viewModel);
            _postAction?.Invoke();
        }
    }

    public class GoogleItemsViewModel : ItemsViewModelBase
    {
        public void AddResult(StreamingRecognizeResponse item)
        {
            if (item.Error != null)
            {
                _AddItem(new GoogleResultItemViewModel(item.Error.Message, Brushes.Red, false, item));
                return;
            }

            var text = string.Join("\n", item.Results.SelectMany(x => x.Alternatives).Select(x => x.Transcript));
            var isPartial = item.Results.Any(x => x.IsFinal) == false;
            var brush = isPartial ? Brushes.Orange : Brushes.Green;
            var model = new GoogleResultItemViewModel(text, brush, isPartial, item);
            if (_items.Count == 0 || _items.Last().IsPartial == false)
            {
                _AddItem(model);
            }
            else
            {
                _items[_items.Count - 1] = model;
            }
        }
    }

    public class AzureItemsViewModel : ItemsViewModelBase
    {
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

        private void _ReplacePartialOrAdd(SpeechEvent item, SolidColorBrush brush)
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
            var model = new AzureResultItemViewModel(overrideText ?? item.Text, brush, item);
            _AddItem(model);
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
