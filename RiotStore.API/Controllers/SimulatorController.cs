using Microsoft.AspNetCore.Mvc;
using RiotStore.API.Services.Interfaces;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulatorController : ControllerBase
    {
        private readonly ISimulatorService _simulatorService;

        public SimulatorController(ISimulatorService simulatorService)
        {
            _simulatorService = simulatorService;
        }

        [HttpPost("single")]
        public async Task<IActionResult> SendSinglePurchase([FromBody] PurchaseAttemptDto purchase)
        {
            await _simulatorService.SimulatePurchaseAttemptAsync(purchase.ProductId, purchase.ProductName, purchase.Quantity);
            return Ok(new { message = "Intento de compra enviado" });
        }

        [HttpPost("batch")]
        public async Task<IActionResult> SendBatchPurchases([FromBody] List<PurchaseAttemptDto> purchases)
        {
            var purchaseList = purchases.Select(p => (p.ProductId, p.ProductName, p.Quantity)).ToList();
            await _simulatorService.SimulateBatchPurchaseAsync(purchaseList);
            return Ok(new { message = $"{purchases.Count} intentos de compra enviados" });
        }

        [HttpPost("batch-metrics")]
        public async Task<IActionResult> SendBatchWithMetrics([FromBody] BatchSimulationRequestDto request)
        {
            if (request.ProductId <= 0 || request.Quantity <= 0 || request.BatchCount <= 0)
            {
                return BadRequest(new { error = "Los parámetros productId, quantity y batchCount deben ser mayores a 0" });
            }

            var metrics = await _simulatorService.SimulateBatchWithMetricsAsync(request.ProductId, request.Quantity, request.BatchCount);
            return Ok(metrics);
        }
    }

    public class PurchaseAttemptDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class BatchSimulationRequestDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int BatchCount { get; set; }
    }
}