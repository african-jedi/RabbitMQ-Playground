using System;

namespace EventBusRabbitMQ;

public class RabbitMQOptions
{
   public string Connection{get;}
   public string Exchange{get;}
   public string QueueName { get; }

   public RabbitMQOptions(string connection, string exchange, string queueName)
   {
      Connection=connection;
      Exchange=exchange;
      QueueName=queueName;
   }
}
