using Shared;
using System.Text;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
        Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;
}

await using var wrapper = await RabbitHelper.CreateExchangeAsync(
    exchangeName: "direct_logs",
    exchangeType: RabbitMQ.Client.ExchangeType.Direct);

await wrapper.CreateQueueAndConsumeAsync(
    routingKeys: args, //severity levels in this case
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