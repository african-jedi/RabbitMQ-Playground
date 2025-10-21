using System;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.ProducerAndConsumer.WebApi.Model;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
   [HttpGet]
   public IActionResult GetOrders()
   {
       // Logic to retrieve orders
       return Ok(new OrderDTO[]
       {
           new OrderDTO { Id = 1, ProductName = "Product A", Quantity = 2 },
           new OrderDTO { Id = 2, ProductName = "Product B", Quantity = 5 }
       });
   }
}
