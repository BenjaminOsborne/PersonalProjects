using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;

namespace Shared;

public static class ChannelHelper
{
    public class ChannelWrapper(IChannel channel, QueueDeclareOk queue, IReadOnlyList<IAsyncDisposable> disposables) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            foreach (var ad in disposables)
            {
                await ad.DisposeAsync();
            }
        }

        public async Task SendMessageAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue.QueueName, body: body);
            Console.WriteLine($" [x] Sent {message}");
        }

        public async Task BasicConsumeAsync(AsyncEventHandler<BasicDeliverEventArgs> fnOnReceivedAsync)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += fnOnReceivedAsync;
            await channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer: consumer);
        }
    }

    public static async Task<ChannelWrapper> CreateAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost", UserName = "rabbitmq_admin_user", Password = "bg0mXyNAOD1vay8VP" };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var queue = await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
        return new(channel, queue, [channel, connection]);
    }

}