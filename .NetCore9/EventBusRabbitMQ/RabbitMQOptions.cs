using System;

namespace RabbitMQ.EventBusRabbitMQ;

public class RabbitMQOptions
{
   public string Connection { get; set; }
   public string Exchange { get; set; }
   public string QueueName { get; set; }

   public RabbitMQOptions(string connection, string exchange, string queueName)
   {
      Connection = connection;
      Exchange = exchange;
      QueueName = queueName;
   }
}
