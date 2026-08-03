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
            return await _context.StockBalances.AsNoTracking().FirstOrDefaultAsync(s => s.product_id == productId);
        }

        public async Task<List<StockBalance>> GetAllAsync()
        {
            return await _context.StockBalances.AsNoTracking().ToListAsync();
        }

        public async Task<StockBalance> UpdateOrCreateAsync(int productId, int initialStock, int totalAttempts, int currentBalance)
        {
            var existing = await _context.StockBalances.FirstOrDefaultAsync(s => s.product_id == productId);

            if (existing != null)
            {
                existing.total_attempts = totalAttempts;
                existing.current_balance = currentBalance;
                existing.last_updated = DateTime.UtcNow;
                _context.StockBalances.Update(existing);
            }
            else
            {
                var stockBalance = new StockBalance
                {
                    product_id = productId,
                    initial_stock = initialStock,
                    total_attempts = totalAttempts,
                    current_balance = currentBalance,
                    status = "ACTIVE",
                    last_updated = DateTime.UtcNow
                };
                _context.StockBalances.Add(stockBalance);
            }

            await _context.SaveChangesAsync();
            return existing ?? new StockBalance
            {
                product_id = productId,
                initial_stock = initialStock,
                total_attempts = totalAttempts,
                current_balance = currentBalance,
                status = "ACTIVE",
                last_updated = DateTime.UtcNow
            };
        }
    }
}