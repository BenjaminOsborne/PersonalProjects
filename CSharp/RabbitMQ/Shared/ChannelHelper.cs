using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;

namespace Shared;

public static class ChannelHelper
{
    public class QueueWrapper(IChannel channel, QueueDeclareOk queue, bool isDurable, IReadOnlyList<IAsyncDisposable> disposables) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() =>
            await _DisposeAsync(disposables);

        public async Task SendMessageAsync(string message)
        {
            await channel.BasicPublishAsync(
                exchange: string.Empty, //"default" exchange
                routingKey: queue.QueueName,
                basicProperties: new BasicProperties { Persistent = isDurable },
                mandatory: true,
                body: Encoding.UTF8.GetBytes(message));
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

    public class ExchangeWrapper(IChannel channel, string exchangeName, IReadOnlyList<IAsyncDisposable> disposables) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() =>
            await _DisposeAsync(disposables);

        public async Task SendMessageAsync(string message, string routingKey = "")
        {
            await channel.BasicPublishAsync(exchange: exchangeName,
                routingKey: routingKey,
                body: Encoding.UTF8.GetBytes(message));
            Console.WriteLine($" [x] Sent {message}");
        }

        public async Task CreateQueueAndConsumeAsync(Func<AsyncEventingBasicConsumer, BasicDeliverEventArgs, Task> fnOnReceivedAsync,
            IReadOnlyList<string>? routingKeys = null)
        {
            var queueResult = await channel.QueueDeclareAsync(); //non-durable, exclusive, autodelete, server-generated-name
            var queueName = queueResult.QueueName;

            if (routingKeys == null || !routingKeys.Any())
            {
                await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: ""); //bind new queue to exchange
            }
            else
            {
                foreach (var routingKey in routingKeys)
                {
                    await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);
                }
            }

            Console.WriteLine($" [*] Waiting for {exchangeName}.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) => fnOnReceivedAsync((AsyncEventingBasicConsumer)model, ea);

            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
        }
    }

    public static async Task<QueueWrapper> CreateQueueAsync(bool durable = true, string queueName = "Example_2")
    {
        var connection = await _GetFactory().CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var queue = await channel.QueueDeclareAsync(queue: queueName, durable: durable, exclusive: false, autoDelete: false, arguments: null);
        return new(channel, queue, durable, [channel, connection]);
    }

    public static async Task<ExchangeWrapper> CreateExchangeAsync(string exchangeName = "logs", string exchangeType = ExchangeType.Fanout)
    {
        var connection = await _GetFactory().CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: exchangeType);

        return new(channel, exchangeName, [channel, connection]);
    }

    private static ConnectionFactory _GetFactory() => new()
    {
        HostName = "localhost",
        UserName = "rabbitmq_admin_user",
        Password = "bg0mXyNAOD1vay8VP"
    };

    private static async Task _DisposeAsync(IReadOnlyList<IAsyncDisposable> disposables)
    {
        foreach (var ad in disposables)
        {
            await ad.DisposeAsync();
        }
    }
}