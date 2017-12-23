using System;

namespace CSharpClient
{
    class Program
    {
        //Username  |Password
        //Ben       |Tester
        //Terry     |Orange

        static void Main(string[] args)
        {
            Console.WriteLine("Enter Username:");
            var username = Console.ReadLine();

            Console.WriteLine("Enter Password:");
            var password = Console.ReadLine();

            var url = "http://localhost:8080/";

            var logger = new WritterLogger(Console.Out);
            var client = new ChatClient(logger);
            client.RunAsync(url, username, password).Wait();

            Console.ReadKey();
        }
    }
}
