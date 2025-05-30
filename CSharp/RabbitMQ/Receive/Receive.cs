using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Shared;

Console.WriteLine("Starting Receive...\n\n");

await using var pair = await ChannelHelper.CreateAsync();
var channel = pair.Channel;

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}. Model is consumer: {model == consumer}");
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();