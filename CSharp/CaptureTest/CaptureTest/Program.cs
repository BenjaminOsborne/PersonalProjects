using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CaptureTest
{
    class Program
    {
        static void Main(string[] args)
        {
            _PrintForLoop();
            _PrintListSelect();
            _PrintFuncListSelect();
            _PrintFuncEnumSelect();
            _WhileFunctionList();

            Console.ReadLine();
        }

        private static void _WhileFunctionList()
        {
            Console.WriteLine("Print While Function");

            List<Func<int>> actions = new List<Func<int>>();

            int variable = 0;
            while (variable < 5)
            {
                actions.Add(() => variable * 2);
                ++variable;
            }

            foreach (var act in actions)
            {
                Console.WriteLine(act.Invoke());
            }
        }

        private static void _PrintFuncListSelect()
        {
            Console.WriteLine("Print Func from List loop");

            var enList = _GetListFuncInts().Select(x => { Console.WriteLine(x()); return x(); });
            var newList = enList.ToList();
        }

        private static void _PrintFuncEnumSelect()
        {
            Console.WriteLine("Print Func From Enum loop");

            var enList = _GetEnumFuncInts().Select(x => { Console.WriteLine(x()); return x(); });
            var newList = enList.ToList();
        }

        private static IEnumerable<Func<int>> _GetListFuncInts()
        {
            var listFuncs = new List<Func<int>>();
            for (int i = 0; i < 10; i++)
            {
                listFuncs.Add(() => i);
            }
            return listFuncs;
        }

        private static IEnumerable<Func<int>> _GetEnumFuncInts()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return () => i;
            }
        }

        private static void _PrintListSelect()
        {
            Console.WriteLine("Select loop");
            var listInts = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                listInts.Add(i);
            }

            var enList = listInts.Select(x => { Console.WriteLine(x); return x; });
            var newList = enList.ToList();
        }

        private static void _PrintForLoop()
        {
            Console.WriteLine("For Loop");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
        }
    }
}
