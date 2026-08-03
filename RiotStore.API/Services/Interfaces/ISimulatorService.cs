using RiotStore.Shared.Dtos;

namespace RiotStore.API.Services.Interfaces
{
    public interface ISimulatorService
    {
        Task SimulatePurchaseAttemptAsync(int productId, string productName, int quantity);
        Task SimulateBatchPurchaseAsync(List<(int productId, string productName, int quantity)> purchases);
        Task<SimulationMetricsDto> SimulateBatchWithMetricsAsync(int quantity, int batchCount);
    }
}