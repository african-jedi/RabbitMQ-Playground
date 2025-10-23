using System;
using RabbitMQ.EventBusRabbitMQ;
using RabbitMQ.EventBusRabbitMQ.Events;
using RabbitMQ.ProducerAndConsumer.WebApi.Model;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Services;

public class OrderService(IEventBus eventBus, ILogger<OrderService> logger) 
{
    public async Task CreateOrder(OrderDTO order)
    {
        logger.LogInformation("Creating order event: {OrderId}", order.Id);
        var @event = new OrderCreatedIntegrationEvent(order.Id);
        await eventBus.PublishEvent(@event);
    }

    public async Task SendOrderForShipment(int orderId)
    {
        logger.LogInformation("Sending ordershipment event: {OrderId}", orderId);
        var shipOrder = new OrderShippedIntegrationEvent(orderId);
        await eventBus.PublishEvent(shipOrder);
    }
}
