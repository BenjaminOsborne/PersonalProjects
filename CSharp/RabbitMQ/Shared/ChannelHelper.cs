using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;

namespace Shared;

public static class ChannelHelper
{
    public class ChannelWrapper(IChannel channel, QueueDeclareOk queue, bool isDurable, IReadOnlyList<IAsyncDisposable> disposables) : IAsyncDisposable
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
            await channel.BasicPublishAsync(exchange: string.Empty,
                routingKey: queue.QueueName,
                basicProperties: new BasicProperties { Persistent = isDurable },
                mandatory: true,
                body: body);
            Console.WriteLine($" [x] Sent {message}");
        }

        public async Task BasicConsumeAsync(Func<AsyncEventingBasicConsumer, BasicDeliverEventArgs, Task> fnOnReceivedAsync,
            bool limitTo1Prefetch = true,
            bool autoAck = true)
        {
            if (limitTo1Prefetch)
            {
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false); //QoS is "Quality of Service"
            }

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                await fnOnReceivedAsync((AsyncEventingBasicConsumer) sender, ea);
                
                if (autoAck == false)
                {
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            await channel.BasicConsumeAsync(queue.QueueName, autoAck: autoAck, consumer: consumer);
        }
    }

    public static async Task<ChannelWrapper> CreateAsync(bool durable = true)
    {
        var factory = new ConnectionFactory { HostName = "localhost", UserName = "rabbitmq_admin_user", Password = "bg0mXyNAOD1vay8VP" };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var queue = await channel.QueueDeclareAsync(queue: "Example_2", durable: durable, exclusive: false, autoDelete: false, arguments: null);
        return new(channel, queue, durable, [channel, connection]);
    }

}