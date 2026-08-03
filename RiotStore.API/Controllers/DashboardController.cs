using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiotStore.Infrastructure.Data;
using RiotStore.API.Dtos;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly RiotStoreDbContext _context;

        public DashboardController(RiotStoreDbContext context)
        {
            _context = context;
        }

        [HttpGet("stock")]
        public async Task<IActionResult> GetAllStock()
        {
            var stocks = await _context.StockBalances
                .AsNoTracking()
                .ToListAsync();

            if (!stocks.Any())
                return Ok(new List<StockBalanceDetailDto>());

            var productIds = stocks.Select(s => s.product_id).ToList();
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.category)
                .Where(p => productIds.Contains(p.product_id))
                .ToListAsync();

            var response = stocks
                .Select(stock =>
                {
                    var product = products.FirstOrDefault(p => p.product_id == stock.product_id);
                    var soldUnits = stock.initial_stock - stock.current_balance;
                    var convRate = stock.total_attempts > 0 ? (double)soldUnits / stock.total_attempts * 100 : 0;
                    var oversell = stock.current_balance < 0 && stock.initial_stock > 0
                        ? Math.Abs((double)stock.current_balance) / stock.initial_stock * 100
                        : 0;
                    var percentRemaining = stock.initial_stock > 0
                        ? (double)stock.current_balance / stock.initial_stock * 100
                        : 0;

                    return new StockBalanceDetailDto
                    {
                        ProductId = stock.product_id,
                        ProductName = product?.name ?? "N/A",
                        ProductSku = product?.sku ?? "N/A",
                        CategoryName = product?.category?.name ?? "Sin categoría",
                        InitialStock = stock.initial_stock,
                        CurrentBalance = stock.current_balance,
                        TotalAttempts = stock.total_attempts,
                        SoldUnits = soldUnits,
                        ConversionRate = Math.Round(convRate, 2),
                        OversellingPercentage = Math.Round(oversell, 2),
                        Status = stock.status,
                        LastUpdated = stock.last_updated,
                        IsOversold = stock.current_balance < 0,
                        PercentageRemaining = Math.Round(percentRemaining, 2)
                    };
                })
                .OrderByDescending(x => x.TotalAttempts)
                .ToList();

            return Ok(response);
        }

        [HttpGet("stock/{productId}")]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            var stock = await _context.StockBalances
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.product_id == productId);

            if (stock == null)
                return NotFound();

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.category)
                .FirstOrDefaultAsync(p => p.product_id == productId);

            var soldUnits = stock.initial_stock - stock.current_balance;
            var convRate = stock.total_attempts > 0 ? (double)soldUnits / stock.total_attempts * 100 : 0;
            var oversell = stock.current_balance < 0 && stock.initial_stock > 0
                ? Math.Abs((double)stock.current_balance) / stock.initial_stock * 100
                : 0;
            var percentRemaining = stock.initial_stock > 0
                ? (double)stock.current_balance / stock.initial_stock * 100
                : 0;

            var response = new StockBalanceDetailDto
            {
                ProductId = stock.product_id,
                ProductName = product?.name ?? "N/A",
                ProductSku = product?.sku ?? "N/A",
                CategoryName = product?.category?.name ?? "Sin categoría",
                InitialStock = stock.initial_stock,
                CurrentBalance = stock.current_balance,
                TotalAttempts = stock.total_attempts,
                SoldUnits = soldUnits,
                ConversionRate = Math.Round(convRate, 2),
                OversellingPercentage = Math.Round(oversell, 2),
                Status = stock.status,
                LastUpdated = stock.last_updated,
                IsOversold = stock.current_balance < 0,
                PercentageRemaining = Math.Round(percentRemaining, 2)
            };

            return Ok(response);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stocks = await _context.StockBalances
                .AsNoTracking()
                .ToListAsync();

            var totalInventory = stocks.Sum(s => s.initial_stock);
            var totalAttempts = stocks.Sum(s => s.total_attempts);
            var totalBalance = stocks.Sum(s => s.current_balance);
            var totalSold = totalInventory - totalBalance;

            var stats = new DashboardStatisticsDto
            {
                TotalInventory = totalInventory,
                TotalAttempts = totalAttempts,
                TotalBalance = totalBalance,
                TotalSold = totalSold,
                OversoldProducts = stocks.Count(s => s.current_balance < 0),
                LowStockProducts = stocks.Count(s => s.current_balance > 0 && s.current_balance <= 10),
                AvailableProducts = stocks.Count(s => s.current_balance > 0),
                ExhaustedProducts = stocks.Count(s => s.current_balance == 0),
                GlobalConversionRate = totalAttempts > 0 ? Math.Round((double)totalSold / totalAttempts * 100, 2) : 0,
                GlobalOverselling = totalInventory > 0 && totalBalance < 0 ? Math.Round(Math.Abs((double)totalBalance) / totalInventory * 100, 2) : 0,
                Timestamp = DateTime.UtcNow
            };

            return Ok(stats);
        }

        [HttpGet("benchmarks")]
        public async Task<IActionResult> GetBenchmarks()
        {
            var benchmarks = await _context.GeneratorBenchmarks
                .AsNoTracking()
                .OrderByDescending(b => b.measured_at)
                .ToListAsync();

            return Ok(benchmarks);
        }
    }
}