using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.EventBusRabbitMQ.Events;
using RabbitMQ.ProducerAndConsumer.WebApi.Model;
using RabbitMQ.ProducerAndConsumer.WebApi.Services;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpGet]
    public async Task<OrderDTO[]> GetOrders()
    {
        return GetList();
    }

    [HttpPost]
    public async Task<Results<CreatedAtRoute<OrderDTO>, BadRequest<string>>> CreateOrder([FromBody] OrderDTO order)
    {
        if (order == null || string.IsNullOrEmpty(order.ProductName) || order.Quantity <= 0)
        {
            return TypedResults.BadRequest("Invalid order data.");
        }

        // Logic to create the order
        // For demonstration, we'll just return the created order with a dummy ID
        order.Id = new Random().Next(1000, 9999);
        await _orderService.CreateOrder(order);

        return TypedResults.CreatedAtRoute<OrderDTO>(routeName: "GetOrderById", routeValues: new { id = order.Id }, value: order);
    }

    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<OrderDTO> GetOrderById(int id)
    {
        return GetList().First(c => c.Id == id);
    }
    
    #region Private Methods
    
    private static OrderDTO[] GetList()
    {
        // Logic to retrieve orders
        return
        [
           new OrderDTO { Id = 1, ProductName = "Product A", Quantity = 2 },
           new OrderDTO { Id = 2, ProductName = "Product B", Quantity = 5 }
        ];
    }

    #endregion
}