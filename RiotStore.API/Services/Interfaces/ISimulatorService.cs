using RiotStore.Shared.Events;

namespace RiotStore.API.Services.Interfaces
{
    public interface ISimulatorService
    {
        Task SimulatePurchaseAttemptAsync(int productId, string productName, int quantity);
        Task SimulateBatchPurchaseAsync(List<(int productId, string productName, int quantity)> purchases);
    }
}