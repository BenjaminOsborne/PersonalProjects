using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using NAudio.Wave;

namespace UI
{
    public class GoogleSpeechClient
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Subject<StreamingRecognizeResponse> _subjectResponses = new Subject<StreamingRecognizeResponse>();

        private readonly WaveInEvent _waveIn;
        private readonly SpeechClient.StreamingRecognizeStream _streamingCall;

        private readonly IDisposable _dispose;
        private readonly Task _streamCallReadTask;

        private GoogleSpeechClient(WaveInEvent waveIn, SpeechClient.StreamingRecognizeStream streamingCall, Subject<StreamingRecognizeRequest> subjectRequests)
        {
            _waveIn = waveIn;
            _streamingCall = streamingCall;

            _dispose = subjectRequests.SubscribeAsync(CurrentThreadScheduler.Instance, x => _streamingCall.WriteAsync(x));

            _streamCallReadTask = Task.Run(async () =>
            {
                var stream = _streamingCall.ResponseStream;
                while (_tokenSource.IsCancellationRequested == false && await stream.MoveNext(default(CancellationToken)))
                {
                    _subjectResponses.OnNext(stream.Current);
                }
            }, _tokenSource.Token);
        }

        public static async Task<GoogleSpeechClient> StartRecording()
        {
            if (WaveIn.DeviceCount < 1)
            {
                throw new Exception("No Audio Device");
            }

            var speech = SpeechClient.Create();
            var streamingCall = speech.StreamingRecognize();
            
            // Write the initial request with the config.
            await streamingCall.WriteAsync(new StreamingRecognizeRequest
            {
                StreamingConfig = new StreamingRecognitionConfig
                {
                    Config = new RecognitionConfig
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = "en",
                    },
                    InterimResults = true,
                }
            });

            var subjectRequests = new Subject<StreamingRecognizeRequest>();
            var waveIn = new WaveInEvent { DeviceNumber = 0, WaveFormat = new WaveFormat(16000, 1) };
            waveIn.DataAvailable += (sender, args) =>
            {
                var content = Google.Protobuf.ByteString.CopyFrom(args.Buffer, 0, args.BytesRecorded);
                var request = new StreamingRecognizeRequest { AudioContent = content };
                subjectRequests.OnNext(request);
            };
            waveIn.RecordingStopped += (sender, args) =>
            {
            };
            waveIn.StartRecording();

            return new GoogleSpeechClient(waveIn, streamingCall, subjectRequests);
        }

        public IObservable<SpeechEvent> GetObserveableEvents() => _subjectResponses.Select(result =>
        {
            if (result.Error != null)
            {
                return new GoogleSpeechEvent(SpeechEventType.Error, WordConfidence.WordsFromError(result.Error.Message), result);
            }

            var words = result.Results.SelectMany(x => x.Alternatives).Select(x => new WordConfidence(x.Transcript, x.Confidence)).ToImmutableList();
            var isPartial = result.Results.Any(x => x.IsFinal) == false;
            var type = isPartial ? SpeechEventType.PartialResponse : SpeechEventType.DictationResponse;
            return new GoogleSpeechEvent(type, words, result);
        });

        public async Task StopRecording()
        {
            _dispose.Dispose();

            _tokenSource.Cancel();
            _tokenSource.Dispose();

            _waveIn.StopRecording();
            await _streamingCall.WriteCompleteAsync();

            await _streamCallReadTask;

            _subjectResponses.OnCompleted();
            _subjectResponses.Dispose();
        }
    }
}
