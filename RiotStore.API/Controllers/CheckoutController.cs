using Microsoft.AspNetCore.Mvc;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.API.DTOs;
using RiotStore.Shared.Events;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(IOrderRepository orderRepository, ILogger<CheckoutController> logger)
        {
            _orderRepository = orderRepository;
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
    }
}