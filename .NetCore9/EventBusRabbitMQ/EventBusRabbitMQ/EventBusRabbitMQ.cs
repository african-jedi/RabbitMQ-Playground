using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using EventBusRabbitMQ.Events;

namespace EventBusRabbitMQ;

public class EventBus : IEventBus, IDisposable, IHostedService
{
    private RabbitMQOptions _options;
    public EventBus(RabbitMQOptions options)
    {
        _options = options;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public async Task<Task> PublishEvent(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;
        var factory = new ConnectionFactory() { HostName = _options.Connection };
        using (var connection = await factory.CreateConnectionAsync())
        {
            using (var channel = await connection.CreateChannelAsync())
            {
                await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Direct);

                var body = SerializeMessage(@event);

                var properties = new BasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = DeliveryModes.Persistent; // persistent

                await channel.BasicPublishAsync(exchange: _options.Exchange,
                                                routingKey: routingKey,
                                                basicProperties: properties,
                                                body: body,
                                                mandatory: true,
                                                cancellationToken: CancellationToken.None);

            }

        }
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var routingKey = "OrderCreatedIntegrationEvent";
        var factory = new ConnectionFactory() { HostName = _options.Connection };
        var connection = factory.CreateConnectionAsync().Result;
        var channel = connection.CreateChannelAsync().Result;

        channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Direct);
        // Declare a queue
        channel.QueueDeclareAsync(queue: _options.QueueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

        //bind queue to exchange with routing key
        channel.QueueBindAsync(queue: _options.QueueName,
                                     exchange: _options.Exchange,
                                     routingKey: routingKey,
                                     arguments: null);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        //Investigate need for JsonSerializerOptions in subscriptionInfo
        //return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);

        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());
    }
}
