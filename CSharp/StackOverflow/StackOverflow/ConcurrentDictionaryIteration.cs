using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace StackOverflow
{
    /// <summary>
    /// This is not a bug!
    /// 1) Concurrent dictionary implements ICollection
    /// 2) "OrderBy" creates a Buffer class which try-casts the source to ICollection and (if succeeds, as in this case)
    /// copies the collection (of "known" length - which then changes!) - to an array variable.
    /// Therefore whilst ConcurrentDictionary is threadsafe on its own terms, other system classes / functions can violate it.
    /// </summary>
    public class ConcurrentDictionaryIteration
    {
        static void Crash()
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
