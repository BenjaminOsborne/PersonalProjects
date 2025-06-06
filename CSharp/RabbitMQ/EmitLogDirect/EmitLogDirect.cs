using Shared;

await using var wrapper = await RabbitHelper.CreateExchangeAsync(
    exchangeName: "direct_logs",
    exchangeType: RabbitMQ.Client.ExchangeType.Direct);

var severity = (args.Length > 0) ? args[0] : "info";
var message = (args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Hello World!";

await wrapper.SendMessageAsync(message, routingKey: severity);
Console.WriteLine($" [x] Sent '{severity}':'{message}'");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();