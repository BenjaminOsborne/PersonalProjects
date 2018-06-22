using System.Collections.Immutable;
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

    public class WordConfidence
    {
        public WordConfidence(string word, double? confidence)
        {
            Word = word;
            Confidence = confidence;
        }

        public string Word { get; }
        public double? Confidence { get; }

        public static ImmutableList<WordConfidence> WordsFromError(string error)
            => ImmutableList.Create(new WordConfidence($"<{error}>", 0.0));
    }

    public abstract class SpeechEvent
    {
        protected SpeechEvent(SpeechEventType type, ImmutableList<WordConfidence> words)
        {
            Type = type;
            Words = words;
        }

        public SpeechEventType Type { get; }
        public ImmutableList<WordConfidence> Words { get; }
        public bool IsPartial => Type == SpeechEventType.PartialResponse;
    }

    public class AzureSpeechEvent : SpeechEvent
    {
        public AzureSpeechEvent(SpeechEventType type, ImmutableList<WordConfidence> words, RecognitionResult result) : base(type, words)
        {
            Result = result;
        }
        public RecognitionResult Result { get; }
    }

    public class GoogleSpeechEvent : SpeechEvent
    {
        public GoogleSpeechEvent(SpeechEventType type, ImmutableList<WordConfidence> words, StreamingRecognizeResponse result) : base(type, words)
        {
            Result = result;
        }
        public StreamingRecognizeResponse Result { get; }
    }
}
