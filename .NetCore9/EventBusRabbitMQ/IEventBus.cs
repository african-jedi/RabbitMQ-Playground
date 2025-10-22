using Microsoft.Extensions.Hosting;

namespace RabbitMQ.EventBusRabbitMQ;

public interface IEventBus: IHostedService
{
   public Task<Task> PublishEvent(IntegrationEvent @event);
}
