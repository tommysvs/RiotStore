using RiotStore.Infrastructure.Data;

namespace RiotStore.API.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
    }
}