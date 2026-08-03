using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace RiotStore.Infrastructure.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly RiotStoreDbContext _context;

        public OrderRepository(RiotStoreDbContext context)
        {
            _context = context;
        }

        public async Task<long> CreateOrderAsync(
            string fullName,
            string email,
            string address,
            string city,
            string state,
            string zipCode,
            List<OrderItemDto> items,
            string paymentMethod)
        {
            try
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.email == email);
                if (client == null)
                {
                    client = new Client
                    {
                        full_name = fullName,
                        email = email,
                        address = $"{address}, {city}, {state} {zipCode}"
                    };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                }

                decimal totalAmount = items.Sum(i => i.UnitPrice * i.Quantity);

                var order = new Order
                {
                    transaction_id = Guid.NewGuid().ToString(),
                    client_id = client.client_id,
                    total_amount = totalAmount,
                    origin = "WEB_UI",
                    status = "PROCESSED",
                    customer_segment = "mid-demand",
                    is_retry = false,
                    total_quantity_requested = items.Sum(i => i.Quantity),
                    created_at = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var orderDetails = items.Select(item => new OrderDetail
                {
                    order_id = order.order_id,
                    product_id = item.ProductId,
                    quantity = item.Quantity,
                    unit_price = item.UnitPrice,
                    subtotal = item.UnitPrice * item.Quantity
                }).ToList();

                _context.OrderDetails.AddRange(orderDetails);
                await _context.SaveChangesAsync();

                return order.order_id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la orden", ex);
            }
        }
    }
}