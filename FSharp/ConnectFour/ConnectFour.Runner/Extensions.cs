using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFour.Runner
{
    public static class Extensions
    {
        public static IEnumerable<int> To(this int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                yield return i;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enItems, Action<T> action)
        {
            foreach (var item in enItems)
            {
                action(item);
            }
        }
    }
}
