using RiotStore.Shared.Events;

namespace RiotStore.Consumer.Services.Interfaces
{
    public interface IOrderProcessingService
    {
        Task ProcessOrderAsync(OrderCreatedEvent orderEvent);
    }
}