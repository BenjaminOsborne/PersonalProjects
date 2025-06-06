using Shared;

Console.WriteLine("Starting Send...\n\n");

await using var wrapper = await RabbitHelper.CreateQueueAsync();

await wrapper.SendMessageAsync(GetMessage(args));

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args) =>
    (args.Length > 0) ? string.Join(" ", args) : "Hello World!";