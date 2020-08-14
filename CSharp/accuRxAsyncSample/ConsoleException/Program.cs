using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleException
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += (sender, __) => Console.WriteLine($"Unobserved!!!:\n{sender}");

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000); // emulate some calculation
                Console.WriteLine("Before exception");
                throw new ArgumentException("Stop it Betsy, you'll ruin the demo...");
            })
                .ContinueWith(t => Console.WriteLine($"Continue... {t.Exception?.Message}"), TaskContinuationOptions.OnlyOnFaulted);

            Thread.Sleep(3000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("Done");
        }
    }
}
