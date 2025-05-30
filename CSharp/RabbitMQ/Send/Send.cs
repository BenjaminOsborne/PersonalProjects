using Shared;

Console.WriteLine("Starting Send...\n\n");

await Task.Delay(1000); //Time for Receive to fire up too...

await using var pair = await ChannelHelper.CreateAsync();

await pair.SendMessageAsync("Hello World 1");

await Task.Delay(1000);

await pair.SendMessageAsync("Hello World 2");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();