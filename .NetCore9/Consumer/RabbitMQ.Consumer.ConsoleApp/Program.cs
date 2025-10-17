using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Program
{
    static async Task Main(string[] args)
    {
        //consumer send to "letterbox" queue which uses default exchange
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
            // consume message from the queue
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");
                await Task.Yield();
            };

            await channel.BasicConsumeAsync(queue: "letterbox",
                                            autoAck: true,
                                            consumer: consumer,
                                            cancellationToken: CancellationToken.None);
            Console.WriteLine($"Consumer started. Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}

