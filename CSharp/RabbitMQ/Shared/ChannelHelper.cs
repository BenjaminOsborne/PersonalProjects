using RabbitMQ.Client;
using System.Text;

namespace Shared;

public static class ChannelHelper
{
    public record ChannelWrapper(IChannel Channel, IReadOnlyList<IAsyncDisposable> Disposables) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            foreach (var ad in Disposables)
            {
                await ad.DisposeAsync();
            }
        }

        public async Task SendMessageAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            await Channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            Console.WriteLine($" [x] Sent {message}");
        }
    }

    public static async Task<ChannelWrapper> CreateAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost", UserName = "rabbitmq_admin_user", Password = "bg0mXyNAOD1vay8VP" };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

        return new(channel, [channel, connection]);
    }

}