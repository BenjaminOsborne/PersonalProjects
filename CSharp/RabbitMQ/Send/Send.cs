using Shared;

Console.WriteLine("Starting Send...\n\n");

await using var wrapper = await RabbitHelper.CreateQueueAsync();

foreach (var n in Enumerable.Range(0, 10))
{
    await Task.Delay(1000); //Still wait on first loop, gives time for Receive to fire up too...

    await wrapper.SendMessageAsync(DateTimeOffset.UtcNow.ToString("O"));
}

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();