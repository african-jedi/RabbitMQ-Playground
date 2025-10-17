using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;

namespace EventBusRabbitMQ;

public class EventBus : IEventBus, IDisposable, IHostedService
{
    private RabbitMQOptions _options;
    private IChannel? _consumerChannel;
    public EventBus(RabbitMQOptions options)
    {
        _options = options;
        _consumerChannel = null;
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
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
        //remove hardcoded routing key
        var routingKey = "OrderCreatedIntegrationEvent";
        var factory = new ConnectionFactory() { HostName = _options.Connection };
        var connection = factory.CreateConnectionAsync().Result;
        _consumerChannel = connection.CreateChannelAsync().Result;

        _consumerChannel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Direct);
        // Declare a queue
        _consumerChannel.QueueDeclareAsync(queue: _options.QueueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

        //bind queue to exchange with routing key
        _consumerChannel.QueueBindAsync(queue: _options.QueueName,
                                     exchange: _options.Exchange,
                                     routingKey: routingKey,
                                     arguments: null);

        //add message received event handler here

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    #region Private Methods
    // private IntegrationEvent DeserializeMessage(string message, Type eventType)
    // {
    //     //return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
    // }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(_consumerChannel, nameof(_consumerChannel));
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            //handle the event
            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            //logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);
        }

        // Even on exception we take the message off the queue.
        // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
        // For more information see: https://www.rabbitmq.com/dlx.html
        await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        // if (logger.IsEnabled(LogLevel.Trace))
        // {
        //     logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        // }

        // await using var scope = serviceProvider.CreateAsyncScope();

        // if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        // {
        //     logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
        //     return;
        // }

        // Deserialize the event
        //var integrationEvent = DeserializeMessage(message, eventType);

        // REVIEW: This could be done in parallel

        // Get all the handlers using the event type as the key
        // foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        // {
        //     await handler.Handle(integrationEvent);
        // }
        await Task.Yield();        
    }
    
    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        //Investigate need for JsonSerializerOptions in subscriptionInfo
        //return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);

        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());
    }

    #endregion
}
