using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpHelperFunctions;

namespace CSharpRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var oRunner = new TriangleRunner();
            oRunner.Run(TriangleData.GetSmallTriange()); //1074
            oRunner.Run(TriangleData.GetBigTriangle()); //7273

            Console.ReadLine();
        }
    }
}
