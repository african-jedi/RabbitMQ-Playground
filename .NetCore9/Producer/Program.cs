using EventBusRabbitMQ;
using EventBusRabbitMQ.Events;

class Program
{
    static async Task Main(string[] args)
    {
        //axample 1
        var factory = new ConnectionFactory() { HostName = "localhost" };
        // Create a connection
        using (var connection = await factory.CreateConnectionAsync())
        {
            // Create a channel
            await using var channel = await connection.CreateChannelAsync();
            // Declare a queue
            await channel.QueueDeclareAsync(queue: "letterbox",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            var message = "Hello, RabbitMQ!";
            var body = Encoding.UTF8.GetBytes(message);

            // Create basic properties
            var properties = new BasicProperties();
            properties.Persistent = true;
            // Publish the message to the queue
            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: "letterbox",
                                            basicProperties: properties,
                                            body: body,
                                            mandatory: true,
                                            cancellationToken: CancellationToken.None);

            Console.WriteLine($"Published message: {message}");

            //Example 2 - send only event and use routing key
            Console.WriteLine("Press [enter] to test event publishing using EventBus.");
            Console.ReadLine();
            IEventBus eventBus = new EventBusRabbitMQ.EventBus(
                new RabbitMQOptions(connection: "localhost", exchange: "event_bus", queueName: "order_queue")
            );

            //start event bus
            await eventBus.StartAsync(CancellationToken.None);


            //publish event
            var @event = new OrderCreatedIntegrationEvent(1);
            await eventBus.PublishEvent(@event);

            Console.WriteLine("Close exclusive connection and channel.");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
