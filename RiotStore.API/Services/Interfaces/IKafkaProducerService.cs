using RiotStore.Shared.Events;

namespace RiotStore.API.Services.Interfaces
{
    public interface IKafkaProducerService
    {
        Task SendOrderCreatedEventAsync(OrderCreatedEvent orderEvent);
    }
}