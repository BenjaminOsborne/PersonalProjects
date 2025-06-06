using Shared;

await using var wrapper = await RabbitHelper.CreateExchangeAsync();

await wrapper.SendMessageAsync(GetMessage(args));

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args) =>
    args.Length > 0 ? string.Join(" ", args) : "info: Hello World!";