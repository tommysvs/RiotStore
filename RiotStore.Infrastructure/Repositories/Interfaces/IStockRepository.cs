using RiotStore.Infrastructure.Data;

namespace RiotStore.Infrastructure.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<StockBalance?> GetByProductIdAsync(int productId);
        Task<List<StockBalance>> GetAllAsync();
        Task<StockBalance> UpdateOrCreateAsync(int productId, int initialStock, int totalAttempts, int currentBalance);
    }
}