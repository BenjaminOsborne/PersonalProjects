using System;
using System.Collections.Immutable;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace UI
{
    public class AzureSpeechClient : IDisposable
    {
        private const string _keyFileName = "AzureKey.txt";
        private const string _defaultLocale = "en-US";

        private readonly Subject<AzureSpeechEvent> _events = new Subject<AzureSpeechEvent>();

        private readonly IDisposable _dispose;

        private AzureSpeechClient(MicrophoneRecognitionClient micClient)
        {
            micClient.OnMicrophoneStatus += _OnMicrophoneStatus;
            micClient.OnPartialResponseReceived += _OnPartialResponseReceivedHandler;
            micClient.OnResponseReceived += _OnMicDictationResponseReceivedHandler;
            micClient.OnConversationError += _OnConversationErrorHandler;

            _dispose = Disposable.Create(() =>
            {
                micClient.EndMicAndRecognition();
                micClient.Dispose();
                _events.OnCompleted();
                _events.Dispose();
            });
        }

        public static AzureSpeechClient Start(string overrideKey = null)
        {
            if (overrideKey != null)
            {
                _WriteKey(overrideKey);
            }

            var key = _GetKey();
            var micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                SpeechRecognitionMode.LongDictation,
                _defaultLocale,
                key);

            var azureClient = new AzureSpeechClient(micClient);
            micClient.StartMicAndRecognition();
            return azureClient;
        }

        public IObservable<SpeechEvent> GetObservableEvents() => _events;

        public void Dispose() => _dispose.Dispose();

        private void _OnNext(SpeechEventType type, ImmutableList<WordConfidence> words)
        {
            var item = new AzureSpeechEvent(type, words, null);
            _events.OnNext(item);
        }

        private void _OnNextDictation(RecognitionResult result)
        {
            var words = result.Results.Select(x => new WordConfidence(x.DisplayText, _ToDouble(x.Confidence))).ToImmutableList();
            var item = words.Any(x => string.IsNullOrEmpty(x.Word) == false)
                ? new AzureSpeechEvent(SpeechEventType.DictationResponse, words, result)
                : new AzureSpeechEvent(SpeechEventType.Error, WordConfidence.WordsFromError(result.RecognitionStatus.ToString()), result);
            _events.OnNext(item);
        }

        private void _OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
            => _OnNext(SpeechEventType.Begin, ImmutableList<WordConfidence>.Empty);

        private void _OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
            => _OnNext(SpeechEventType.PartialResponse, ImmutableList.Create(new WordConfidence(e.PartialResult, _ToDouble(Confidence.Normal))));

        private void _OnMicDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            _OnNextDictation(e.PhraseResponse);

            var status = e.PhraseResponse.RecognitionStatus;
            if (status == RecognitionStatus.EndOfDictation || status == RecognitionStatus.DictationEndSilenceTimeout)
            {
                _OnNext(SpeechEventType.End, ImmutableList<WordConfidence>.Empty);
            }
        }

        private void _OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
            => _OnNext(SpeechEventType.Error, WordConfidence.WordsFromError($"Code: {e.SpeechErrorCode}\tText: {e.SpeechErrorText}"));

        private static void _WriteKey(string key)
        {
            using (var store = _GetStore())
            {
                using (var stream = new IsolatedStorageFileStream(_keyFileName, FileMode.Create, store))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(key);
                    }
                }
            }
        }

        private static double _ToDouble(Confidence confidence)
        {
            switch (confidence)
            {
                case Confidence.None:
                    return 0;
                case Confidence.Low:
                    return 0.33;
                case Confidence.Normal:
                    return 0.66;
                case Confidence.High:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(confidence), confidence, null);
            }
        }

        private static string _GetKey()
        {
            try
            {
                using (var store = _GetStore())
                {
                    using (var stream = new IsolatedStorageFileStream(_keyFileName, FileMode.Open, store))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            return reader.ReadLine();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static IsolatedStorageFile _GetStore() => IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, (Type)null, (Type)null);
    }
}
