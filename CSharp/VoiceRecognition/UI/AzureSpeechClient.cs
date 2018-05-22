using System;
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

        private void _OnNext(SpeechEventType type, string text = "")
        {
            var item = new AzureSpeechEvent(type, text, null);
            _events.OnNext(item);
        }

        private void _OnNextDictation(RecognitionResult result)
        {
            var text = string.Join("\n", result.Results.Select(x => x.DisplayText));
            var item = string.IsNullOrEmpty(text)
                ? new AzureSpeechEvent(SpeechEventType.Error, result.RecognitionStatus.ToString(), result)
                : new AzureSpeechEvent(SpeechEventType.DictationResponse, text, result);
            _events.OnNext(item);
        }

        private void _OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
            => _OnNext(SpeechEventType.Begin);

        private void _OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
            => _OnNext(SpeechEventType.PartialResponse, e.PartialResult);

        private void _OnMicDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            _OnNextDictation(e.PhraseResponse);

            var status = e.PhraseResponse.RecognitionStatus;
            if (status == RecognitionStatus.EndOfDictation || status == RecognitionStatus.DictationEndSilenceTimeout)
            {
                _OnNext(SpeechEventType.End);
            }
        }

        private void _OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
            => _OnNext(SpeechEventType.Error, $"Code: {e.SpeechErrorCode}\tText: {e.SpeechErrorText}");

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
