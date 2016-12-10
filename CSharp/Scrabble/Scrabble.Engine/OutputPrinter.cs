using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Scrabble.Engine
{
    public enum PrintMode
    {
        Debug = 0,
        Console
    }

    public static class OutputPrinter
    {
        public static PrintMode Mode { get; set; }

        static OutputPrinter()
        {
            Mode = PrintMode.Debug;
        }

        public static void WriteLine(string sLine)
        {
            if (Mode == PrintMode.Debug)
            {
                Debug.WriteLine(sLine);
            }
            else if (Mode == PrintMode.Console)
            {
                Console.WriteLine(sLine);
            }
        }

        public static void Write(string sLine)
        {
            if (Mode == PrintMode.Debug)
            {
                Debug.Write(sLine);
            }
            else if (Mode == PrintMode.Console)
            {
                Console.Write(sLine);
            }
        }
    }
}
