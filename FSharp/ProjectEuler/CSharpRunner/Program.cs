using System;
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
