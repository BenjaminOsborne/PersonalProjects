using Shared;
using System.Text;

Console.WriteLine("Starting Receive...\n\n");

await using var wrapper = await RabbitHelper.CreateQueueAsync();

Console.WriteLine(" [*] Waiting for messages.");

await wrapper.BasicConsumeAsync(async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var pair = RabbitHelper.SplitMessageWithNow(message);
    var elMs = (DateTimeOffset.UtcNow - pair.sentAt).TotalMilliseconds;
    Console.WriteLine($" [x] Received {pair.msg}. Elapsed (ms): {elMs}");
    
    await Task.Delay(100);
    Console.WriteLine(" [x] Done");
},
    limitTo1Prefetch: true,
    autoAck: true);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();