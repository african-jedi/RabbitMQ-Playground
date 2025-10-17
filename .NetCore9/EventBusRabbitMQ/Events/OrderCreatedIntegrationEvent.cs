using System;

namespace EventBusRabbitMQ.Events;

public record OrderCreatedIntegrationEvent(int OrderId):IntegrationEvent;