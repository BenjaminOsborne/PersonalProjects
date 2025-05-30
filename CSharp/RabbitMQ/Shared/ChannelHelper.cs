using RabbitMQ.Client;

namespace Shared;

public static class ChannelHelper
{
    public record ChannelPair(IChannel Channel, IReadOnlyList<IAsyncDisposable> Disposables) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            foreach (var ad in Disposables)
            {
                await ad.DisposeAsync();
            }
        }
    }

    public static async Task<ChannelPair> CreateAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost", UserName = "rabbitmq_admin_user", Password = "bg0mXyNAOD1vay8VP" };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

        return new(channel, [channel, connection]);
    }

}