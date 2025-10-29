using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.EventBusRabbitMQ.Abstractions;

namespace RabbitMQ.EventBusRabbitMQ;

public sealed class EventBus(
    IServiceProvider serviceProvider,
    IOptions<RabbitMQOptions> options,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions,
    ILogger<EventBus> logger
    ) : IEventBus, IDisposable, IHostedService
{
    private readonly RabbitMQOptions _options = options.Value;
    private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionOptions.Value;
    private IChannel? _consumerChannel;

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
                await channel.ExchangeDeclareAsync(_options.Exchange
                , ExchangeType.Direct
                , arguments: new Dictionary<string, object?>
                                                {
                                                    { "alternate-exchange", $"alternate-{_options.Exchange}" }
                                                });

                var body = SerializeMessage(@event);

                var properties = new BasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = DeliveryModes.Persistent; // persistent

                await channel.BasicPublishAsync(exchange: _options.Exchange,
                                                routingKey: routingKey,
                                                basicProperties: properties,
                                                body: body,
                                                mandatory: false,
                                                cancellationToken: CancellationToken.None);

            }
        }
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Factory.StartNew((c) =>
        {
            try
            {
                string alternateExchangeName = $"alternate-{_options.Exchange}";
                string alternateQueueName = "alternate-queue";

                var factory = new ConnectionFactory() { HostName = _options.Connection };
                var connection = factory.CreateConnectionAsync().Result;
                _consumerChannel = connection.CreateChannelAsync().Result;

                _consumerChannel.ExchangeDeclareAsync(alternateExchangeName
                , ExchangeType.Fanout);

                _consumerChannel.ExchangeDeclareAsync(_options.Exchange
                , ExchangeType.Direct
                , arguments: new Dictionary<string, object?>
                                                {
                                                    { "alternate-exchange", alternateExchangeName }
                                                });

                //declare the alternate queue
                _consumerChannel.QueueDeclareAsync(queue: alternateQueueName,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);
                
                _consumerChannel.QueueBindAsync(
                        queue: alternateQueueName,
                        exchange: alternateExchangeName,
                        routingKey: "");

                var altConsumer = new AsyncEventingBasicConsumer(_consumerChannel);
                altConsumer.ReceivedAsync += async (sender, eventArgs) =>
                {
                    var message = Encoding.UTF8.GetString(eventArgs.Body.Span);
                    logger.LogWarning("Received message in alternate exchange: {Message}", message);
                    await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
                };

                _consumerChannel.BasicConsumeAsync(
                    queue: alternateQueueName,
                    autoAck: false,
                    consumer: altConsumer);
             
                //declare the main queue
                _consumerChannel.QueueDeclareAsync(queue: _options.QueueName,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);

                if (logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace($"Starting RabbitMQ queue basic consume with routing key: {_options.QueueName}");

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.ReceivedAsync += OnMessageReceived;
                _consumerChannel.BasicConsumeAsync(
                    queue: _options.QueueName,
                    autoAck: false,
                    consumer: consumer);

                //bind queue to exchange with routing key
                foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
                {
                    _consumerChannel.QueueBindAsync(
                        queue: _options.QueueName,
                        exchange: _options.Exchange,
                        routingKey: eventName);
                }

                //add message received event handler here
            }
            catch (OperationCanceledException canceledException)
            {
                logger.LogError(canceledException, "Operation cancelled");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting RabbitMQ connection");
            }
        },
        TaskCreationOptions.LongRunning, cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var channel = _consumerChannel;
            if (channel?.IsOpen == true)
                await channel.CloseAsync(cancellationToken);
                
            _consumerChannel?.Dispose();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Channel closing was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping RabbitMQ connection");
        }
        //todo: test
        return;
    }

    #region Private Methods
    private IntegrationEvent DeserializeMessage(string message, Type eventType)
    {
        var result = JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
        if (result is null)
            throw new InvalidOperationException($"Failed to deserialize message to {eventType.FullName}");

        return result;
    }

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
            logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);
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

        await using var scope = serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        // Deserialize the event
        var integrationEvent = DeserializeMessage(message, eventType);

        // REVIEW: This could be done in parallel

        // Get all the handlers using the event type as the key
        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            await handler.Handle(integrationEvent);
        }
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
