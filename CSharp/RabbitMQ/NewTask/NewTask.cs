using Shared;

Console.WriteLine("Starting Send...\n\n");

await using var pair = await ChannelHelper.CreateAsync();

await pair.SendMessageAsync(GetMessage(args));

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args) =>
    (args.Length > 0) ? string.Join(" ", args) : "Hello World!";