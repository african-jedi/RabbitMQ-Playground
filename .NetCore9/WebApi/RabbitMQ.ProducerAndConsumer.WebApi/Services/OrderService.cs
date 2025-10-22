using System;
using RabbitMQ.EventBusRabbitMQ;
using RabbitMQ.EventBusRabbitMQ.Events;
using RabbitMQ.ProducerAndConsumer.WebApi.Model;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Services;

public class OrderService
{
    private readonly IEventBus _eventBus;
    public OrderService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateOrder(OrderDTO order)
    {
        var @event = new OrderCreatedIntegrationEvent(order.Id);
        await _eventBus.PublishEvent(@event);
    }
}
