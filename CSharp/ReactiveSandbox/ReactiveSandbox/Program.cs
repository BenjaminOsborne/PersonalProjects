using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace ReactiveSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var programCase = 9;
            switch(programCase)
            {
                case 1:
                    _SubscribeSequence();
                    break;
                case 2:
                    _GroupUserInputStream();
                    break;
                case 3:
                    _ParallelExecution();
                    break;
                case 4:
                    _CanCancelSequence();
                    break;
                case 5:
                    _BufferAndSample();
                    break;
                case 6:
                    _Window();
                    break;
                case 7:
                    _Join();
                    break;
                case 8:
                    _HotObservable();
                    break;
                case 9:
                    _DistributeWorkBasic();
                    break;
                case 10:
                    PingPongProgram.Invoke();
                    break;
                case 11:
                    LogicGates.Invoke();
                    break;
            }

            Console.WriteLine("End application...");
            Console.ReadLine();
        }

        private static int _ThreadID => Thread.CurrentThread.ManagedThreadId;
        
        private static void _SubscribeSequence()
        {
            Console.WriteLine("Initial Thread: " + _ThreadID);

            bool wait = true;
            Enumerable.Range(0, 25).ToObservable(NewThreadScheduler.Default)
                                   .Finally(() =>
                                   {
                                       Console.WriteLine("Finally...");
                                       wait = false;
                                   })
                                   .Subscribe(x => Console.WriteLine(x + " " + Thread.CurrentThread.ManagedThreadId));
            while (wait)
            {
                Thread.Sleep(100);
            }
        }

        private static void _GroupUserInputStream()
        {
            _GetInputs().ToObservable(NewThreadScheduler.Default)
                        .GroupBy(x => x.Length).Subscribe(g =>
                        {
                            Console.WriteLine("New Group: " + g.Key + " on thread: " + _ThreadID);
                            var length = 1;
                            g.Subscribe(a => Console.WriteLine($"Group {g.Key} grown to {length++} by {a}"));
                        });

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private static async void _ParallelExecution()
        {
            var observable = Observable.CombineLatest(
                Observable.Start(() => _WritelineAndReturn("A")),
                Observable.Start(() => _WritelineAndReturn("B")),
                Observable.Start(() => _WritelineAndReturn("C"))
            ).Finally(() => Console.WriteLine("Done!"));

            Console.WriteLine("Pre enumerate...");
            
            var firstAsync = observable.FirstAsync();
            foreach (var r in await firstAsync)
                Console.WriteLine(r);
        }

        private static async void _CanCancelSequence()
        {
            IDisposable observable = Observable.Create<int>(o =>
            {
                var cancel = new CancellationDisposable(); // internally creates a new CancellationTokenSource
                NewThreadScheduler.Default.Schedule(() =>
                {
                    int i = 0;
                    while(true)
                    {
                        Thread.Sleep(250);  // here we do the long lasting background operation
                        if (!cancel.Token.IsCancellationRequested) // check cancel token periodically
                        {
                            Console.WriteLine($"OnNext ({i} to {++i})");
                            o.OnNext(i);
                        }
                        else
                        {
                            Console.WriteLine("Aborting because cancel event was signaled!");
                            o.OnCompleted(); // will not make it to the subscriber
                            return;
                        }
                    }
                });
                return cancel;
            })
            .Finally(() => Console.WriteLine("\nFinally!\n"))
            .Subscribe(i => Console.Write(i + ", "));

            using (observable)
            {
                Thread.Sleep(5000);
            }
        }

        private static void _BufferAndSample()
        {
            var interval = Observable.Interval(TimeSpan.FromSeconds(0.25)).Publish();

            interval.Connect();
            Thread.Sleep(2000);

            interval.Subscribe(a => Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + a));
            Thread.Sleep(2000);

            interval.Sample(TimeSpan.FromSeconds(1)).Subscribe(a =>
            {
                Thread.Sleep(3000);
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " Sample: " + a);
            });
            Thread.Sleep(2000);

            //interval.Buffer(TimeSpan.FromSeconds(2.1)).Subscribe(a => Console.WriteLine("Buffered: " + string.Join(",", a)));

            while (true)
            {
                Thread.Sleep(1000 * 1000);
            }
        }

        private static void _Window()
        {
            Observable.Interval(TimeSpan.FromSeconds(0.25))
                      .Window(() => Observable.Interval(TimeSpan.FromSeconds(3)))
                      .Subscribe(a =>
                      {
                          Console.WriteLine("\nWindow start: " + DateTime.Now + "\n");
                          a.Subscribe(b => Console.Write(b + ", "));
                      });
        }

        private static void _Join()
        {
            var left = 1.To(30);
            var right = 25.To(40);

            var join = left.ToObservable().Join(right.ToObservable(), l => Observable.Never<int>(), r => Observable.Never<int>(), (l,r) => new { L = l, R = r });
            join.Subscribe(a =>
            {
                if (a.R == a.L)
                {
                    Console.WriteLine(a.R);
                }
            });
        }

        private static void _HotObservable()
        {
            Func<DateTime> fnGetVersion = () => System.IO.File.GetLastWriteTime(@"C:\TEMP\SomeFile.txt");
            
            Console.WriteLine($"Calling Thread: {Thread.CurrentThread.ManagedThreadId}");

            var startVersion = fnGetVersion();
            var seed = Tuple.Create(startVersion, startVersion);
            var connectable = Observable.Interval(TimeSpan.FromSeconds(0.25))
                                        .Scan(seed, (time, _) => Tuple.Create(time.Item2, fnGetVersion()))
                                        .Where(x => x.Item2 > x.Item1)
                                        .Select(x =>
                                        {
                                            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}. Subscriber Version Changed: {x.Item1} to {x.Item2}");
                                            return x.Item2;
                                        })
                                        .Publish();

            var connected = connectable.Connect();

            Console.WriteLine($"Calling Thread: {Thread.CurrentThread.ManagedThreadId}");

            Console.WriteLine("Listener 1 online");
            var listener1 = connectable.Subscribe(onNext: x => _PrintOnNext(x, 1));

            Thread.Sleep(2 * 1000);

            Console.WriteLine("Listener 2 - 3 online");
            var listener2 = connectable.ObserveOn(NewThreadScheduler.Default).Subscribe(onNext: x => _PrintOnNext(x, 2));
            var listener3 = connectable.ObserveOn(Scheduler.CurrentThread).Subscribe(onNext: x => _PrintOnNext(x, 3));

            _SleepForSeconds(2);

            Console.WriteLine("Listener 1 disposed");
            listener1.Dispose();

            _SleepForSeconds(10);

            Console.WriteLine("Listener 2 - 3 disposed");
            listener2.Dispose();
            listener3.Dispose();

            connected.Dispose();

            Console.WriteLine($"Calling Thread: {Thread.CurrentThread.ManagedThreadId}");
        }

        private static void _SleepForSeconds(int sleepSeconds)
        {
            var dt = DateTime.Now;
            while (DateTime.Now < dt.AddSeconds(sleepSeconds))
            {
                Thread.Sleep(500);
            }
        }

        private static void _PrintOnNext(DateTime dt, int n)
        {
            Console.WriteLine($"Listener {n} (Thread {Thread.CurrentThread.ManagedThreadId}) Version Changed: {dt}");
        }

        private static string _WritelineAndReturn(string id)
        {
            Console.WriteLine($"Executing Thread {Thread.CurrentThread.ManagedThreadId}: {id}");
            return "Result " + id;
        }

        public static IEnumerable<string> _GetInputs()
        {
            while (true)
                yield return Console.ReadLine();
        }

        private static void _DistributeWorkBasic()
        {
            var s1 = new Subject<int>(); var s2 = new Subject<int>(); var s3 = new Subject<int>();
            var result = Observable.Amb(s1, s2, s3);

            result.Subscribe(Console.WriteLine, () => Console.WriteLine("Completed"));

            s1.OnNext(1);
            s2.OnNext(2);
            s3.OnNext(3);
            s1.OnNext(1);
            s2.OnNext(2);
            s3.OnNext(3);
            s3.OnNext(3);
            s2.OnNext(2);
            s1.OnNext(1);
            s2.OnNext(2);
            s2.OnNext(2);

            s1.OnCompleted();
            s2.OnCompleted();
            s3.OnCompleted();
        }
    }

    public static class Extensions
    {
        public static IEnumerable<int> To(this int start, int stop)
        {
            var current = start;
            while (current <= stop)
            {
                yield return current++;
            }
        }
    }
}
