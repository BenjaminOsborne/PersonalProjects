using Shared;

await using var wrapper = await RabbitHelper.CreateExchangeAsync();

await wrapper.CreateQueueAndConsumeAsync((model, ea) =>
{
    var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($" [x] {message}");
    return Task.CompletedTask;
});

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();