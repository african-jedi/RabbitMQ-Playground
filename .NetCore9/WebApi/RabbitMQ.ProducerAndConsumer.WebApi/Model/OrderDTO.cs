using System;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Model;

public class OrderDTO
{
   public int Id { get; set; }
   public string? ProductName { get; set; }
   public int Quantity { get; set; }
}
