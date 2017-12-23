using System.Reactive.Concurrency;

namespace ChatServiceLayer
{
    public interface ISchedulerProvider
    {
        IScheduler ThreadPool { get; }
    }

    public class SchedulerProvider : ISchedulerProvider
    {
        public IScheduler ThreadPool { get; } = ThreadPoolScheduler.Instance;
    }
}
