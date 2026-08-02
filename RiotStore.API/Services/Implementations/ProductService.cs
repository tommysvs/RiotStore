using RiotStore.API.Services.Interfaces;
using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;

namespace RiotStore.API.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetProductByIdAsync(productId);
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _productRepository.GetProductsByCategoryAsync(categoryId);
        }
    }
}