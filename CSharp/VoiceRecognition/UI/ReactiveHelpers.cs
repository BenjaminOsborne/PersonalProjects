using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace UI
{
    public static class ReactiveHelpers
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> obs, IScheduler scheduler, Func<T, Task> onNext, Action onCompleted = null)
            => _SubscribeAsync(obs, scheduler, onNext, onCompleted);

        public static IDisposable SubscribeAsync<T, TRet>(this IObservable<T> obs, IScheduler scheduler, Func<T, Task<TRet>> onNext, Action onCompleted = null)
            => _SubscribeAsync(obs, scheduler, onNext, onCompleted);

        private static IDisposable _SubscribeAsync<T, TTask>(IObservable<T> obs, IScheduler scheduler, Func<T, TTask> onNext, Action onCompleted) where TTask : Task
        {
            //Must both observe on scheduler and move work after async onto scheduler.
            //Essential when scheduler is dispatcher to ensure work always done on UI thread
            var seq = obs
                .ObserveOn(scheduler)
                .Select(x => Observable.FromAsync(() => onNext(x), scheduler));
            var flatten = seq.Concat();
            return onCompleted != null
                ? flatten.Subscribe(_ => { }, onCompleted)
                : flatten.Subscribe();
        }

        public static IObservable<TRes> SelectAsync<T, TRes>(this IObservable<T> obs, Func<T, Task<TRes>> fnGetTask, IScheduler scheduler)
        {
            return obs
                .ObserveOn(scheduler)
                .Select(x => Observable.FromAsync(() => fnGetTask(x), scheduler))
                .Concat();
        }
    }
}
