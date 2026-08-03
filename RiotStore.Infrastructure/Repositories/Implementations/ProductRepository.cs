using Microsoft.EntityFrameworkCore;
using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;

namespace RiotStore.Infrastructure.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly RiotStoreDbContext _context;

        public ProductRepository(RiotStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.category)
                .Where(p => p.is_active)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.category)
                .FirstOrDefaultAsync(p => p.product_id == productId);
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.category)
                .Where(p => p.category_id == categoryId && p.is_active)
                .ToListAsync();
        }
    }
}