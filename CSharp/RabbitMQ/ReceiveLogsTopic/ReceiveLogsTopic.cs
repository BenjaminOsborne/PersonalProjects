using Shared;
using System.Text;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: {0} [binding_key...]",
        Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;
}

await using var wrapper = await ChannelHelper.CreateExchangeAsync(
    exchangeName: "topic_logs",
    exchangeType: RabbitMQ.Client.ExchangeType.Topic);

await wrapper.CreateQueueAndConsumeAsync(
    routingKeys: args,
    fnOnReceivedAsync: (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var routingKey = ea.RoutingKey;
        Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
        return Task.CompletedTask;
    });

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();