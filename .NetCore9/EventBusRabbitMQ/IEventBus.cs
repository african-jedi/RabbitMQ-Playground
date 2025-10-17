using Microsoft.Extensions.Hosting;

namespace EventBusRabbitMQ;

public interface IEventBus: IHostedService
{
   public Task<Task> PublishEvent(IntegrationEvent @event);
}
