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
        private readonly IStockRepository _stockRepository;
        private readonly RiotStoreDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IStockRepository stockRepository,
            RiotStoreDbContext context,
            ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _stockRepository = stockRepository;
            _context = context;
            _logger = logger;
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
            
            _logger.LogInformation($"Producto: {product.name} (ID: {productId}), Stock DB: {stock?.current_balance}, CurrentStock var: {currentStock}");
            
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

            _logger.LogInformation($"================================");
            _logger.LogInformation($"TOTAL STOCKS EN BD: {allStocks.Count}");
            foreach (var s in allStocks)
            {
                _logger.LogInformation($"  - ProductID: {s.product_id}, Balance: {s.current_balance}, Initial: {s.initial_stock}");
            }
            _logger.LogInformation($"================================");

            var stockDict = allStocks.ToDictionary(s => s.product_id);

            var enriched = new List<object>();

            foreach (var product in products)
            {
                var hasStock = stockDict.TryGetValue(product.product_id, out var stock);
                var currentStock = stock?.current_balance ?? 0;
                var isAvailable = currentStock > 0;
                
                _logger.LogInformation($"PRODUCTO: {product.name} (ID: {product.product_id})");
                _logger.LogInformation($"  - ¿Encontrado en dict?: {hasStock}");
                _logger.LogInformation($"  - Stock object NULL?: {stock == null}");
                _logger.LogInformation($"  - CurrentStock value: {currentStock}");
                _logger.LogInformation($"  - IsAvailable: {isAvailable}");
                
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