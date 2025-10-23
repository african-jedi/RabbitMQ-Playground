using System;

namespace RabbitMQ.EventBusRabbitMQ.Events;

public record OrderShippedIntegrationEvent(int OrderId) : IntegrationEvent;