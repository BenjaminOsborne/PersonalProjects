using System.Text;
using Shared;

Console.WriteLine("Starting Receive...\n\n");

await using var wrapper = await RabbitHelper.CreateQueueAsync();

Console.WriteLine(" [*] Waiting for messages.");

await wrapper.BasicConsumeAsync(async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");

    var dots = message.Split('.').Length - 1;
    await Task.Delay(dots * 1000);

    Console.WriteLine(" [x] Done");
});

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();