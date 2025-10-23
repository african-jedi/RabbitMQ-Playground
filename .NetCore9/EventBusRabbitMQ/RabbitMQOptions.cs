using System;

namespace RabbitMQ.EventBusRabbitMQ;

public class RabbitMQOptions
{
   public string Connection { get; set; }
   public string Exchange { get; set; }
   public string QueueName { get; set; }
}
