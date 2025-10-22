using System;

namespace RabbitMQ.EventBusRabbitMQ.Events;

public record OrderCreatedIntegrationEvent(int OrderId):IntegrationEvent;