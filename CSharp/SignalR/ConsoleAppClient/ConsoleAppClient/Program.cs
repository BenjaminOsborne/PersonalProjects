﻿using System;
using Microsoft.AspNet.SignalR.Client;

namespace ConsoleAppClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://127.0.0.1:8088/"; // "http://localhost:8080/";

            //Set connection
            var connection = new HubConnection(url);
            var hub = "MyHubName";// "MyHub";
            var myHub = connection.CreateHubProxy(hub);

            Console.WriteLine("Enter your name");
            string name = Console.ReadLine();

            connection.Start().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}", task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");

                    myHub.On<string, string>("addMessage", (s1, s2) => {
                        Console.WriteLine(s1 + ": " + s2);
                    });

                    while (true)
                    {
                        string message = Console.ReadLine();

                        if (string.IsNullOrEmpty(message))
                        {
                            break;
                        }

                        myHub.Invoke<string>("Send", name, message).ContinueWith(task1 => {
                            if (task1.IsFaulted)
                            {
                                Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                            }
                            else
                            {
                                Console.WriteLine(task1.Result);
                            }
                        });
                    }
                }

            }).Wait();

            Console.Read();
            connection.Stop();
        }
    }
}
