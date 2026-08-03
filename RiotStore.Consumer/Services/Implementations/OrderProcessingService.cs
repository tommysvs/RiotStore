using Microsoft.EntityFrameworkCore;
using RiotStore.Consumer.Services.Interfaces;
using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.Shared.Events;

namespace RiotStore.Consumer.Services.Implementations
{
    public class OrderProcessingService : IOrderProcessingService
    {
        private readonly RiotStoreDbContext _context;
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(
            RiotStoreDbContext context,
            IStockRepository stockRepository,
            ILogger<OrderProcessingService> logger)
        {
            _context = context;
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task ProcessOrderAsync(OrderCreatedEvent orderEvent)
        {
            try
            {
                if (orderEvent?.Items == null || !orderEvent.Items.Any())
                {
                    _logger.LogWarning($"Evento vacío recibido");
                    return;
                }

                var client = await _context.Clients.FirstOrDefaultAsync(c => c.client_id == orderEvent.CustomerId);
                
                if (client == null)
                {
                    client = new Client
                    {
                        client_id = orderEvent.CustomerId,
                        full_name = $"Customer {orderEvent.CustomerId}",
                        email = $"customer{orderEvent.CustomerId}@riotstore.local",
                        address = "Not specified",
                        segment = orderEvent.CustomerSegment ?? "mid-demand",
                        created_at = DateTime.UtcNow
                    };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cliente creado: {client.client_id}");
                }

                var order = new Order
                {
                    transaction_id = Guid.NewGuid().ToString(),
                    client_id = client.client_id,
                    total_amount = orderEvent.TotalAmount,
                    origin = "KAFKA",
                    status = "PROCESSED",
                    customer_segment = orderEvent.CustomerSegment ?? "mid-demand",
                    product_category = orderEvent.ProductCategory ?? "Sin categoría",
                    is_retry = orderEvent.IsRetry,
                    original_order_id = orderEvent.OriginalOrderId,
                    total_quantity_requested = orderEvent.TotalQuantityRequested,
                    created_at = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in orderEvent.Items)
                {
                    await ProcessOrderItemAsync(order, item, orderEvent);
                }

                _logger.LogInformation($"Orden procesada: {order.order_id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error procesando orden: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private async Task ProcessOrderItemAsync(Order order, OrderItemDto item, OrderCreatedEvent orderEvent)
        {
            var processingStartTime = DateTime.UtcNow;
            
            try
            {
                var product = await _context.Products
                    .Include(p => p.category)
                    .FirstOrDefaultAsync(p => p.product_id == item.ProductId);

                if (product == null)
                {
                    _logger.LogWarning($"Producto no encontrado: {item.ProductId}");
                    return;
                }

                var currentStock = await _stockRepository.GetByProductIdAsync(item.ProductId);
                
                if (currentStock == null || currentStock.current_balance < item.Quantity)
                {
                    _logger.LogWarning($"Stock insuficiente - Producto: {item.ProductId}, " +
                        $"Disponible: {currentStock?.current_balance ?? 0}, Solicitado: {item.Quantity}");
                    
                    var failedAttempt = new PurchaseAttempt
                    {
                        order_id = order.order_id,
                        product_id = item.ProductId,
                        quantity_requested = item.Quantity,
                        product_category = orderEvent.ProductCategory ?? "Sin categoría",
                        customer_segment = orderEvent.CustomerSegment ?? "mid-demand",
                        is_retry = orderEvent.IsRetry,
                        original_order_id = orderEvent.IsRetry ? orderEvent.OriginalOrderId : null,
                        status = "FAILED_OUT_OF_STOCK",
                        attempted_at = processingStartTime,
                        processed_at = DateTime.UtcNow
                    };
                    
                    _context.PurchaseAttempts.Add(failedAttempt);
                    await _context.SaveChangesAsync();
                    return;
                }

                var orderDetail = new OrderDetail
                {
                    order_id = order.order_id,
                    product_id = item.ProductId,
                    quantity = item.Quantity,
                    unit_price = item.UnitPrice,
                    subtotal = item.Quantity * item.UnitPrice
                };

                _context.OrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();

                var purchaseAttempt = new PurchaseAttempt
                {
                    order_id = order.order_id,
                    product_id = item.ProductId,
                    quantity_requested = item.Quantity,
                    product_category = orderEvent.ProductCategory ?? "Sin categoría",
                    customer_segment = orderEvent.CustomerSegment ?? "mid-demand",
                    is_retry = orderEvent.IsRetry,
                    original_order_id = orderEvent.IsRetry ? orderEvent.OriginalOrderId : null,
                    status = "SUCCESS",
                    attempted_at = processingStartTime,
                    processed_at = DateTime.UtcNow
                };

                _context.PurchaseAttempts.Add(purchaseAttempt);
                await _context.SaveChangesAsync();

                var newBalance = currentStock.current_balance - item.Quantity;
                await _stockRepository.UpdateOrCreateAsync(
                    item.ProductId,
                    currentStock.initial_stock,
                    currentStock.total_attempts + 1,
                    newBalance);

                _logger.LogInformation($"Ítem procesado exitosamente - Orden: {order.order_id}, Producto: {item.ProductId}, Cantidad: {item.Quantity}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error procesando ítem - Producto: {item.ProductId}. Error: {ex.Message}");
                throw;
            }
        }
    }
}