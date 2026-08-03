using Microsoft.AspNetCore.Mvc;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.Infrastructure.Data;
using RiotStore.API.DTOs;
using RiotStore.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IStockRepository _stockRepository;
        private readonly RiotStoreDbContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            IOrderRepository orderRepository,
            IStockRepository stockRepository,
            RiotStoreDbContext context,
            ILogger<CheckoutController> logger)
        {
            _orderRepository = orderRepository;
            _stockRepository = stockRepository;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request)
        {
            try
            {
                _logger.LogInformation("Checkout request received");

                if (request?.Customer == null || request.Items == null || !request.Items.Any())
                {
                    _logger.LogWarning("Invalid checkout request - missing customer or items");
                    return BadRequest(new { error = "Customer y items son requeridos" });
                }

                var stockValidation = await ValidateStockAsync(request.Items);
                if (!stockValidation.AllValid)
                {
                    _logger.LogWarning($"Stock validation failed: {string.Join(", ", stockValidation.InvalidItems)}");
                    return BadRequest(new
                    {
                        error = "Stock insuficiente",
                        details = stockValidation.InvalidItems
                    });
                }

                var orderItems = request.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                }).ToList();

                _logger.LogInformation($"Processing checkout with {orderItems.Count} items");

                var orderId = await _orderRepository.CreateOrderAsync(
                    request.Customer.FullName,
                    request.Customer.Email,
                    request.Customer.Address,
                    request.Customer.City,
                    request.Customer.State,
                    request.Customer.ZipCode,
                    orderItems,
                    request.PaymentMethod
                );

                _logger.LogInformation($"Order created successfully with ID: {orderId}");
                return Ok(new { orderId, message = "Orden procesada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Checkout error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { error = $"Error al procesar el pedido: {ex.Message}" });
            }
        }

        private async Task<StockValidationResponse> ValidateStockAsync(List<CartItemDto> items)
        {
            var invalidItems = new List<string>();

            var allStocks = await _context.StockBalances
                .AsNoTracking()
                .ToListAsync();

            var stockDict = allStocks.ToDictionary(s => s.product_id);

            foreach (var item in items)
            {
                stockDict.TryGetValue(item.ProductId, out var stock);
                var currentBalance = stock?.current_balance ?? 0;
                
                if (currentBalance < item.Quantity)
                {
                    invalidItems.Add($"{item.Name}: solicitadas {item.Quantity}, disponibles {currentBalance}");
                    _logger.LogWarning($"Stock insuficiente - {item.Name}: solicitadas {item.Quantity}, disponibles {currentBalance}");
                }
            }

            return new StockValidationResponse
            {
                AllValid = invalidItems.Count == 0,
                InvalidItems = invalidItems
            };
        }
    }

    public class StockValidationResponse
    {
        public bool AllValid { get; set; }
        public List<string> InvalidItems { get; set; } = new();
    }
}