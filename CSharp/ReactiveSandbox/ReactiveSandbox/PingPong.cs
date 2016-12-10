using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveSandbox
{
    public static class PingPongProgram
    {
        public static void Invoke()
        {
            var ping = new Ping();
            var pong = new Pong();

            Console.WriteLine("Press any key to stop ...");

            var pongSubscription = ping.Subscribe(pong);
            var pingSubscription = pong.Subscribe(ping);

            Console.ReadKey();

            pongSubscription.Dispose();
            pingSubscription.Dispose();

            Console.WriteLine("Ping Pong has completed.");
        }
    }

    class Ping : ISubject<Pong, Ping>
    {
        #region Implementation of IObserver<Ping>

        public void OnNext(Pong value)
        {
            Console.WriteLine("Ping received Pong.");
        }

        public void OnError(Exception exception)
        {
            Console.WriteLine("Ping experienced an exception and had to quit playing.");
        }

        public void OnCompleted()
        {
            Console.WriteLine("Ping finished.");
        }

        #endregion

        #region Implementation of IObservable<Pong>

        public IDisposable Subscribe(IObserver<Ping> observer)
        {
            return Observable.Interval(TimeSpan.FromSeconds(2))
                             .Where(n => n < 10)
                             .Select(n => this)
                             .Subscribe(observer);
        }

        public void Dispose()
        {
            OnCompleted();
        }

        #endregion
    }

    class Pong : ISubject<Ping, Pong>
    {
        #region Implementation of IObserver<Ping>

        public void OnNext(Ping value)
        {
            Console.WriteLine("Pong received Ping.");
        }

        public void OnError(Exception exception)
        {
            Console.WriteLine("Pong experienced an exception and had to quit playing.");
        }

        public void OnCompleted()
        {
            Console.WriteLine("Pong finished.");
        }

        #endregion

        #region Implementation of IObservable<Pong>

        public IDisposable Subscribe(IObserver<Pong> observer)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1.5))
                             .Where(n => n < 10)
                             .Select(n => this)
                             .Subscribe(observer);
        }

        public void Dispose()
        {
            OnCompleted();
        }

        #endregion
    }
}
