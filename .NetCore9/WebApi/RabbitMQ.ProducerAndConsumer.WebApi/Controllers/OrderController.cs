using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.ProducerAndConsumer.WebApi.Model;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    [HttpGet]
    public async Task<OrderDTO[]> GetOrders()
    {
        // Logic to retrieve orders
        return
        [
           new OrderDTO { Id = 1, ProductName = "Product A", Quantity = 2 },
           new OrderDTO { Id = 2, ProductName = "Product B", Quantity = 5 }
        ];
    }

    [HttpPost]
    public async Task<Results<CreatedAtRoute, BadRequest<string>, ProblemHttpResult>> CreateOrder([FromBody] OrderDTO order)
    {
        if (order == null || string.IsNullOrEmpty(order.ProductName) || order.Quantity <= 0)
        {
            return TypedResults.BadRequest("Invalid order data.");
        }

        // Logic to create the order
        // For demonstration, we'll just return the created order with a dummy ID
        order.Id = new Random().Next(1000, 9999);

        return TypedResults.CreatedAtRoute("GetOrderById", new { id = order.Id }, order);
    }
}