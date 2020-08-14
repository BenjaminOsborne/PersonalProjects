using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAsync
{
    class Program
    {
        private static HashSet<int> _allIds = new HashSet<int>();

        static async Task Main(string[] args)
        {
            while (true)
            {
                await Task.Delay(500);
                _TrackThreadId();
            }
        }

        private static void _TrackThreadId()
        {
            var cur = Thread.CurrentThread.ManagedThreadId;
            _allIds.Add(cur);
            Console.WriteLine($"Thread Id: {cur}\t[total seen: {_allIds.Count}]");
        }
    }
}

#region Hidden...
//SynchronizationContext.SetSynchronizationContext(new CustomContext());
//SynchronizationContext.SetSynchronizationContext(new NastyDispatcher());

/*
var tasks = Enumerable.Range(0, 10).Select(x => Task.Delay(500)).ToArray();
await Task.WhenAll(tasks);
break;
 */
#endregion
