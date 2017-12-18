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

            var writer = Console.Out;
            var client = new CommonClient(writer);
            client.RunAsync("http://localhost:8080/", username, password).Wait();

            Console.ReadKey();
        }
    }
}
