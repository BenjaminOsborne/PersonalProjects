using RabbitMQ.Client;
using System.Text;
using Shared;

Console.WriteLine("Starting Send...\n\n");

const string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

await using var pair = await ChannelHelper.CreateAsync();
var channel = pair.Channel;

await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();