using Microsoft.AspNetCore.Mvc;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.API.Dtos;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public DashboardController(IStockRepository stockRepository, IProductRepository productRepository)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        [HttpGet("stock/{productId}")]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            var stock = await _stockRepository.GetByProductIdAsync(productId);
            if (stock == null)
                return NotFound();

            var product = await _productRepository.GetProductByIdAsync(productId);

            var response = new StockBalanceDetailDto
            {
                ProductId = stock.product_id,
                ProductName = product?.name ?? "Desconocido",
                ProductSku = product?.sku ?? "N/A",
                InitialStock = stock.initial_stock,
                CurrentBalance = stock.current_balance,
                TotalAttempts = stock.total_attempts,
                ConversionRate = stock.total_attempts > 0
                    ? Math.Round((double)(stock.initial_stock - stock.current_balance) / stock.total_attempts * 100, 2)
                    : 0,
                OversellingPercentage = stock.current_balance < 0
                    ? Math.Round(Math.Abs((double)stock.current_balance) / stock.initial_stock * 100, 2)
                    : 0,
                Status = stock.status,
                LastUpdated = stock.last_updated,
                IsOversold = stock.current_balance < 0,
                PercentageRemaining = stock.initial_stock > 0
                    ? Math.Round((double)stock.current_balance / stock.initial_stock * 100, 2)
                    : 0
            };

            return Ok(response);
        }

        [HttpGet("all-stock")]
        public async Task<IActionResult> GetAllStock()
        {
            var stocks = await _stockRepository.GetAllAsync();
            var products = await _productRepository.GetAllProductsAsync();

            var response = stocks.Select(stock =>
            {
                var product = products.FirstOrDefault(p => p.product_id == stock.product_id);
                var soldUnits = stock.initial_stock - stock.current_balance;

                return new StockBalanceDetailDto
                {
                    ProductId = stock.product_id,
                    ProductName = product?.name ?? "Desconocido",
                    ProductSku = product?.sku ?? "N/A",
                    InitialStock = stock.initial_stock,
                    CurrentBalance = stock.current_balance,
                    TotalAttempts = stock.total_attempts,
                    SoldUnits = soldUnits,
                    ConversionRate = stock.total_attempts > 0
                        ? Math.Round((double)soldUnits / stock.total_attempts * 100, 2)
                        : 0,
                    OversellingPercentage = stock.current_balance < 0
                        ? Math.Round(Math.Abs((double)stock.current_balance) / stock.initial_stock * 100, 2)
                        : 0,
                    Status = stock.status,
                    LastUpdated = stock.last_updated,
                    IsOversold = stock.current_balance < 0,
                    PercentageRemaining = stock.initial_stock > 0
                        ? Math.Round((double)stock.current_balance / stock.initial_stock * 100, 2)
                        : 0
                };
            }).OrderByDescending(x => x.TotalAttempts).ToList();

            return Ok(response);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stocks = await _stockRepository.GetAllAsync();

            var totalInventory = stocks.Sum(s => s.initial_stock);
            var totalAttempts = stocks.Sum(s => s.total_attempts);
            var totalBalance = stocks.Sum(s => s.current_balance);
            var totalSold = totalInventory - totalBalance;
            var oversoldProducts = stocks.Count(s => s.current_balance < 0);
            var lowStockProducts = stocks.Count(s => s.current_balance > 0 && s.current_balance <= 10);

            var stats = new DashboardStatisticsDto
            {
                TotalInventory = totalInventory,
                TotalAttempts = totalAttempts,
                TotalBalance = totalBalance,
                TotalSold = totalSold,
                OversoldProducts = oversoldProducts,
                LowStockProducts = lowStockProducts,
                GlobalConversionRate = totalAttempts > 0
                    ? Math.Round((double)totalSold / totalAttempts * 100, 2)
                    : 0,
                GlobalOverselling = totalInventory > 0 && totalBalance < 0
                    ? Math.Round(Math.Abs((double)totalBalance) / totalInventory * 100, 2)
                    : 0,
                AvailableProducts = stocks.Count(s => s.current_balance > 0),
                ExhaustedProducts = stocks.Count(s => s.current_balance == 0),
                Timestamp = DateTime.UtcNow
            };

            return Ok(stats);
        }
    }
}