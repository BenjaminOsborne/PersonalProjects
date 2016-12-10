using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Async
{
    class Program
    {
        static void Main(string[] args)
        {
            //_RunCSharpMain();

            FSharpAsync.DownloadHelper();
        }

        private static void _RunCSharpMain()
        {
            Console.WriteLine("Program Start");
            AsyncProcessor.PrintThreadID();
            var oProcessor = new AsyncProcessor();
            var oTask = oProcessor.DoTaskProcess();

            Console.WriteLine("Continue with Main");
            {
                //Some work
            }

            Console.WriteLine("Main now waiting...");
            var nResult = oTask.Result;
            Console.WriteLine("Result From Main: " + nResult);

            Console.WriteLine("Program End");
            Console.ReadLine();
        }
    }

    public class AsyncProcessor
    {
        public async Task<int> DoTaskProcess()
        {
            Console.WriteLine("Starting inner...");

            var task1 = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Pause start");
                PrintThreadID();
                Thread.Sleep(2000);
                Console.WriteLine("Pause end");
                return 1;
            });

            Console.WriteLine("Waiting inner...");
            var result = await task1;

            Console.WriteLine("Exiting inner...");

            return result;
        }

        public static void PrintThreadID()
        {
            Console.WriteLine("Thread ID " + Thread.CurrentThread.ManagedThreadId);
        }
    }
}
