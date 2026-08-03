using RiotStore.Shared.Events;

namespace RiotStore.API.Services.Interfaces
{
    public interface IDataGeneratorService
    {
        Task<OrderCreatedEvent> GenerateSinglePurchaseAttemptAsync();

        Task<List<OrderCreatedEvent>> GenerateBatchAsync(
            int count,
            string? targetProductCategory = null,
            bool simulatePeakHour = false);
    }
}