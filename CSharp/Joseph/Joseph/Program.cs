using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joseph
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Joseph typed:");
            var sTyped = Console.ReadLine();
            Console.WriteLine(string.Format("Here is Joseph's Wisdom: {0}", sTyped));
            Console.ReadLine();
        }
    }
}
