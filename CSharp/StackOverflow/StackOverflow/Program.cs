using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
{
    public class Program
    {
        static void Main()
        {
            var dictionary = new ConcurrentDictionary<int, string>();

            var random = new Random();

            Task.Run(() =>
            {
                while (true)
                {
                    var key = random.Next(0, 10);
                    dictionary.TryAdd(key, $"{key}");
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    var key = random.Next(0, 10);
                    string ignore;
                    dictionary.TryRemove(key, out ignore);
                }
            });

            while (true)
            {
                var orderedKeyValues = dictionary.OrderBy(x => x.Key);
                foreach (var kvp in orderedKeyValues)
                {
                    Console.WriteLine($"{kvp.Key} {kvp.Value}");
                }
            }
        }
    }
}