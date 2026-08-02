using Microsoft.EntityFrameworkCore;
using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;

namespace RiotStore.Infrastructure.Repositories.Implementations
{
    public class StockRepository : IStockRepository
    {
        private readonly RiotStoreDbContext _context;

        public StockRepository(RiotStoreDbContext context)
        {
            _context = context;
        }

        public async Task<StockBalance?> GetByProductIdAsync(int productId)
        {
            return await _context.StockBalances.FirstOrDefaultAsync(s => s.product_id == productId);
        }

        public async Task<List<StockBalance>> GetAllAsync()
        {
            return await _context.StockBalances.ToListAsync();
        }

        public async Task<StockBalance> UpdateOrCreateAsync(int productId, int initialStock, int totalAttempts, int currentBalance)
        {
            var existing = await GetByProductIdAsync(productId);

            if (existing != null)
            {
                existing.total_attempts = totalAttempts;
                existing.current_balance = currentBalance;
                existing.status = currentBalance <= 0 ? "OUT_OF_STOCK" : currentBalance < 5 ? "LOW_STOCK" : "IN_STOCK";
                existing.last_updated = DateTime.UtcNow;
                _context.StockBalances.Update(existing);
            }
            else
            {
                var newBalance = new StockBalance
                {
                    product_id = productId,
                    initial_stock = initialStock,
                    total_attempts = totalAttempts,
                    current_balance = currentBalance,
                    status = currentBalance <= 0 ? "OUT_OF_STOCK" : "IN_STOCK",
                    last_updated = DateTime.UtcNow
                };
                _context.StockBalances.Add(newBalance);
                existing = newBalance;
            }

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}