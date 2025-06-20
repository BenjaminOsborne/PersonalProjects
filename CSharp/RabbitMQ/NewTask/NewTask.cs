using Shared;

Console.WriteLine("Starting Send...\n\n");

await using var wrapper = await RabbitHelper.CreateQueueAsync();

var start = DateTimeOffset.UtcNow;
foreach (var loop in Enumerable.Range(2000, 18))
{
    Console.WriteLine($"Sending (ms): {(DateTimeOffset.UtcNow - start).TotalMilliseconds}");
    await wrapper.SendMessageAsync(RabbitHelper.MessageWithNow($"Message:{loop}"));
}

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args) =>
    (args.Length > 0) ? string.Join(" ", args) : "Hello World!";