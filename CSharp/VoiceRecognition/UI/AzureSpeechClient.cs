using System;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace UI
{
    public class AzureSpeechClient : IDisposable
    {
        private const string _defaultLocale = "en-US";
        private const string _key = "<enter key>";

        private readonly MicrophoneRecognitionClient _micClient;
        private readonly Subject<AzureSpeechEvent> _events = new Subject<AzureSpeechEvent>();
        
        public AzureSpeechClient()
        {
            var client = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                SpeechRecognitionMode.LongDictation,
                _defaultLocale,
                _key);
            client.OnMicrophoneStatus += _OnMicrophoneStatus;
            client.OnPartialResponseReceived += _OnPartialResponseReceivedHandler;
            client.OnResponseReceived += _OnMicDictationResponseReceivedHandler;
            client.OnConversationError += _OnConversationErrorHandler;

            client.StartMicAndRecognition();

            _micClient = client;
        }

        public IObservable<SpeechEvent> GetObservableEvents() => _events;

        public void Dispose()
        {
            _micClient?.Dispose();
            _events?.Dispose();
        }

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
                _micClient.EndMicAndRecognition();
            }
        }

        private void _OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
            => _OnNext(SpeechEventType.Error, $"Code: {e.SpeechErrorCode}\tText: {e.SpeechErrorText}");

        public void Poke() => _OnNext(SpeechEventType.End);
    }
}
