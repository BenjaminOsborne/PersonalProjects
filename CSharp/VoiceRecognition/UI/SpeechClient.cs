using System;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace UI
{
    public enum SpeechEventType
    {
        None = 0,
        Begin,
        PartialResponse,
        DictationResponse,
        End,
        Error
    }

    public class SpeechEvent
    {
        public SpeechEvent(SpeechEventType type, string text, RecognitionResult result)
        {
            Type = type;
            Text = text;
            Result = result;
        }

        public SpeechEventType Type { get; }
        public string Text { get; }
        public RecognitionResult Result { get; }
    }

    public class SpeechClient : IDisposable
    {
        private readonly MicrophoneRecognitionClient _micClient;
        private readonly Subject<SpeechEvent> _events = new Subject<SpeechEvent>();

        public SpeechClient()
        {
            _micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                SpeechRecognitionMode.LongDictation,
                DefaultLocale,
                SubscriptionKey);

            _micClient.OnMicrophoneStatus += _OnMicrophoneStatus;
            _micClient.OnPartialResponseReceived += _OnPartialResponseReceivedHandler;
            _micClient.OnResponseReceived += _OnMicDictationResponseReceivedHandler;
            _micClient.OnConversationError += _OnConversationErrorHandler;

            _micClient.StartMicAndRecognition();
        }

        public string DefaultLocale => "en-US";
        public string SubscriptionKey => "28a9163b363a4626b907f917942de08f";

        public IObservable<SpeechEvent> GetObservableEvents() => _events;

        public void Dispose()
        {
            _micClient?.Dispose();
            _events?.Dispose();
        }

        private void _OnNext(SpeechEventType type, string text = "")
        {
            var item = new SpeechEvent(type, text, null);
            _events.OnNext(item);
        }

        private void _OnNextDictation(RecognitionResult result)
        {
            var text = new StringBuilder();
            text.AppendLine($"Status: {result.RecognitionStatus}");
            foreach (var phrase in result.Results)
            {
                text.AppendLine($"Confidene: {phrase.Confidence}\t{phrase.DisplayText}");
            }
            var item = new SpeechEvent(SpeechEventType.DictationResponse, text.ToString(), result);
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
