using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            _azureClient = AzureSpeechClient.Start();
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
                window.Hide();
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

    public class ResultItemViewModel
    {
        public ResultItemViewModel(ImmutableList<WordDisplayViewModel> displayModels, bool isPartial, SpeechEvent result)
        {
            DisplayModels = displayModels;
            IsPartial = isPartial;
            Result = result;
        }

        public ImmutableList<WordDisplayViewModel> DisplayModels { get; }
        public bool IsPartial { get; }
        public SpeechEvent Result { get; }
    }

    public class WordDisplayViewModel
    {
        public WordDisplayViewModel(string word, double confidence)
        {
            Word = word;
            Confidence = confidence;
        }

        public string Word { get; }
        public double Confidence { get; }

        public Brush Brush => _ToBrush(Confidence);

        private static Brush _ToBrush(double confidence)
        {
            if (confidence < 0.33)
            {
                return Brushes.Red;
            }
            if (confidence < 0.66)
            {
                return Brushes.Orange;
            }
            if (confidence < 0.1)
            {
                return Brushes.Yellow;
            }
            return Brushes.Green;
        }
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
                    _AddItem(item);
                    break;
                case SpeechEventType.PartialResponse:
                    _ReplacePartialOrAdd(item);
                    break;
                case SpeechEventType.DictationResponse:
                    _ReplacePartialOrAdd(item);
                    break;
                case SpeechEventType.End:
                    _AddItem(item);
                    break;
                case SpeechEventType.Error:
                    _AddItem(item);
                    break;
                case SpeechEventType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void _ReplacePartialOrAdd(SpeechEvent item)
        {
            if (_items.Count == 0 || _items.Last().IsPartial == false)
            {
                _AddItem(item);
            }
            else
            {
                _items[_items.Count - 1] = _ToModel(item);
            }
        }

        private void _AddItem(SpeechEvent item)
        {
            var viewModel = _ToModel(item);
            _preAction?.Invoke();
            _items.Add(viewModel);
            _postAction?.Invoke();
        }

        private ResultItemViewModel _ToModel(SpeechEvent item)
            => new ResultItemViewModel(_ToDisplay(item.Words), item.IsPartial, item);

        private ImmutableList<WordDisplayViewModel> _ToDisplay(ImmutableList<WordConfidence> itemWords)
            => itemWords.Select(x => new WordDisplayViewModel(x.Word, x.Confidence)).ToImmutableList();
    }
    
    public class ScrollingTextMockViewModel
    {
        public ScrollingTextMockViewModel()
        {
            var items = new[]
            {
                new ResultItemViewModel(new []
                {
                    _Create("Test", 1),
                    _Create("text", 0.8),
                    _Create("bonkers", 0.7),
                    _Create("yeah", 0.5),
                    _Create("blahuterf", 0.1),
                    _Create("!<>", 0),
                }.ToImmutableList(), false, null),
                new ResultItemViewModel(ImmutableList.Create(_Create("More", 0.5)), true, null),
            };
            AzureItems = items;
            GoogleItems = items;
        }

        public IEnumerable<ResultItemViewModel> AzureItems { get; }
        public IEnumerable<ResultItemViewModel> GoogleItems { get; }

        private static WordDisplayViewModel _Create(string word, double confidence) => new WordDisplayViewModel(word, confidence);
    }
}
