using Google.Cloud.Speech.V1;
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

    public abstract class SpeechEvent
    {
        protected SpeechEvent(SpeechEventType type, string text)
        {
            Type = type;
            Text = text;
        }

        public SpeechEventType Type { get; }
        public string Text { get; }
        public bool IsPartial => Type == SpeechEventType.PartialResponse;
    }

    public class AzureSpeechEvent : SpeechEvent
    {
        public AzureSpeechEvent(SpeechEventType type, string text, RecognitionResult result) : base(type, text)
        {
            Result = result;
        }
        public RecognitionResult Result { get; }
    }

    public class GoogleSpeechEvent : SpeechEvent
    {
        public GoogleSpeechEvent(SpeechEventType type, string text, StreamingRecognizeResponse result) : base(type, text)
        {
            Result = result;
        }
        public StreamingRecognizeResponse Result { get; }
    }
}
