using Microsoft.AspNetCore.Mvc;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly RiotStoreDbContext _context;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IStockRepository stockRepository,
            RiotStoreDbContext context,
            ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productRepository.GetAllProductsAsync();
            var productsWithStock = await EnrichProductsWithStockAsync(products);
            return Ok(productsWithStock);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            var productsWithStock = await EnrichProductsWithStockAsync(products);
            return Ok(productsWithStock);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            var stock = await _context.StockBalances
                .AsNoTracking()
                .Where(s => s.product_id == productId)
                .FirstOrDefaultAsync();
            
            var currentStock = stock?.current_balance ?? 0;
            
            var productWithStock = new
            {
                product.product_id,
                product.name,
                product.description,
                product.price,
                product.sku,
                product.image_url,
                product.category,
                CurrentStock = currentStock,
                IsAvailable = currentStock > 0
            };

            return Ok(productWithStock);
        }

        private async Task<List<object>> EnrichProductsWithStockAsync(List<Infrastructure.Data.Product> products)
        {
            var allStocks = await _context.StockBalances
                .AsNoTracking()
                .ToListAsync();

            var stockDict = allStocks.ToDictionary(s => s.product_id);

            var enriched = new List<object>();

            foreach (var product in products)
            {
                var hasStock = stockDict.TryGetValue(product.product_id, out var stock);
                var currentStock = stock?.current_balance ?? 0;
                var isAvailable = currentStock > 0;
                
                enriched.Add(new
                {
                    product.product_id,
                    product.name,
                    product.description,
                    product.price,
                    product.sku,
                    product.image_url,
                    product.category,
                    CurrentStock = currentStock,
                    IsAvailable = isAvailable
                });
            }

            return enriched;
        }
    }
}