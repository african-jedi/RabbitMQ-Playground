using System;
using RabbitMQ.EventBusRabbitMQ;
using RabbitMQ.EventBusRabbitMQ.Events;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Services;

public class OrderService
{
    private readonly IEventBus _eventBus;
    public OrderService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateOrder(int orderId)
    {
        var @event = new OrderCreatedIntegrationEvent(orderId);
        await _eventBus.PublishEvent(@event);
    }
}
