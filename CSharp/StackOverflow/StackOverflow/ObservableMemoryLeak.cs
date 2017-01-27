using System;
using System.Reactive.Linq;

namespace StackOverflow
{
    public class ObservableMemoryLeak
    {
        public static void Leak()
        {
            IObservable<string> obs = Observable.Generate(1, x => x < 1000 * 1000, x => x + 1, x => x.ToString(), x => TimeSpan.FromMilliseconds(500));
            obs.Subscribe(x => { /*Do nothing but simply run the observable*/ });
            Console.ReadLine();
        }

    }
}
