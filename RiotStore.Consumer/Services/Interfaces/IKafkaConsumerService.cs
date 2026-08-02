using RiotStore.Shared.Events;

namespace RiotStore.Consumer.Services.Interfaces
{
    public interface IKafkaConsumerService
    {
        Task StartConsumingAsync(CancellationToken cancellationToken);
    }
}