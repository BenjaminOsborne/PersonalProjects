using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 1;

            const string a = "Foo";

            int j = 2;
            int k = i + j;
        }

        [Foo("More")]
        public string Hello { get; }

        [Foo("Yeah")]
        public int Foo;
    }

    public class FooAttribute : Attribute
    {
        public FooAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}
