using System;
using RabbitMQ.ProducerAndConsumer.WebApi.Services;

namespace RabbitMQ.ProducerAndConsumer.WebApi.Extensions;

//using FluentValidation;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration.GetSection("EventBus");

        builder.AddRabbitMQEventBus("EventBus")
               .AddEventBusSubscriptions();

        services.AddScoped<OrderService>();
    }

    /// <summary>
    /// Todo: add your event bus subscriptions here
    /// </summary>
    /// <param name="eventBus"></param>
    private static void AddEventBusSubscriptions(this IEventBusBuilder eventBus)
    {
        // Here you can register your event bus subscriptions
    }
}

