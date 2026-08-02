using Microsoft.AspNetCore.Mvc;
using RiotStore.Infrastructure.Repositories.Interfaces;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IStockRepository _stockRepository;

        public DashboardController(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        [HttpGet("stock/{productId}")]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            var stock = await _stockRepository.GetByProductIdAsync(productId);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpGet("all-stock")]
        public async Task<IActionResult> GetAllStock()
        {
            var stocks = await _stockRepository.GetAllAsync();
            return Ok(stocks);
        }
    }
}