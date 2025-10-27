using System;

namespace RabbitMQ.EventBusRabbitMQ;

public class RabbitMQOptions
{
   public string Connection { get; set; } = string.Empty;
   public string Exchange { get; set; } = string.Empty;
   public string QueueName { get; set; } = string.Empty;
}
