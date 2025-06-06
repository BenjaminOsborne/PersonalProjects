using System.Text;
using Shared;

Console.WriteLine("Starting Receive...\n\n");

await using var wrapper = await RabbitHelper.CreateQueueAsync();

Console.WriteLine(" [*] Waiting for messages.");

await wrapper.BasicConsumeAsync((model, ea) =>
{
    var body = ea.Body.ToArray();
    var dt = DateTimeOffset.Parse(Encoding.UTF8.GetString(body));
    var elMs = (DateTimeOffset.UtcNow - dt).TotalMilliseconds;
    Console.WriteLine($" [x] Received {dt}. Millisecs: {elMs}. Model: {model?.GetType()}");
    return Task.CompletedTask;
});

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();