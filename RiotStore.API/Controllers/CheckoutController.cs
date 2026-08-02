using Microsoft.AspNetCore.Mvc;
using RiotStore.API.DTOs;
using RiotStore.Infrastructure.Repositories.Interfaces;

namespace RiotStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public CheckoutController(
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request)
        {
            try
            {
                if (request?.Customer == null || request.Items == null || request.Items.Count == 0)
                {
                    return BadRequest(new { message = "Datos de checkout inválidos" });
                }

                var orderItems = new List<OrderItemDto>();
                foreach (var item in request.Items)
                {
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        return BadRequest(new { message = $"Producto {item.ProductId} no encontrado" });
                    }

                    orderItems.Add(new OrderItemDto
                    {
                        ProductId = item.ProductId,
                        Name = item.Name,
                        Sku = product.sku,
                        Price = item.Price,
                        Quantity = item.Quantity
                    });
                }

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

                return Ok(new { orderId = orderId, message = "Pedido procesado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al procesar el pedido", error = ex.Message });
            }
        }
    }
}