﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Please enter your username...");
            var userName = Console.ReadLine();
            Console.WriteLine($"{userName} has entered the building.");

            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:44340/chat")
                .Build();

            connection.On<string>("onConnected", OnConnected);
            connection.On<string>("onDisconnected", OnDisconnected);
            connection.On<string, string>("broadcastMessage", OnSend);

            await connection.StartAsync();

            while (true)
            {
                var msg = Console.ReadLine();
                await connection.InvokeAsync("Send", userName, msg);
            }
        }

        private static void OnConnected(string message) => _ConnectedChanged("OnConnected", message);

        private static void OnDisconnected(string message) => _ConnectedChanged("OnDisconnected", message);

        private static void _ConnectedChanged(string context, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{context}: {message}");
            Console.ResetColor();
        }

        private static void OnSend(string name, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Message from {name}\n{message}\n");
            Console.ResetColor();
        }
    }
}
