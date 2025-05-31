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
    var dt = DateTimeOffset.Parse(Encoding.UTF8.GetString(body));
    var elMs = (DateTimeOffset.UtcNow - dt).TotalMilliseconds;
    Console.WriteLine($" [x] Received {dt}. Millisecs: {elMs}. Model is consumer: {model == consumer}");
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();