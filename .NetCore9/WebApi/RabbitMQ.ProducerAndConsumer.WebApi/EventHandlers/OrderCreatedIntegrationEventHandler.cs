using System;
using RabbitMQ.EventBusRabbitMQ.Abstractions;
using RabbitMQ.EventBusRabbitMQ.Events;
using RabbitMQ.ProducerAndConsumer.WebApi.Services;

namespace RabbitMQ.ProducerAndConsumer.WebApi.EventHandlers;

public class OrderCreatedIntegrationEventHandler(
    OrderService orderService,
    ILogger<OrderCreatedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task Handle(OrderCreatedIntegrationEvent @event)
    {
        // Handle the event
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await orderService.SendOrderForShipment(@event.OrderId);
    }
}
