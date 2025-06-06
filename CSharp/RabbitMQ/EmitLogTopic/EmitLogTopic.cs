using Shared;

await using var wrapper = await RabbitHelper.CreateExchangeAsync(
    exchangeName: "topic_logs",
    exchangeType: RabbitMQ.Client.ExchangeType.Topic);

var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
var message = (args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Hello World!";
await wrapper.SendMessageAsync(routingKey: routingKey, message: message);
Console.WriteLine($" [x] Sent '{routingKey}':'{message}'");