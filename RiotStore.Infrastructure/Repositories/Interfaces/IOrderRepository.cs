using RiotStore.Infrastructure.Data;

namespace RiotStore.Infrastructure.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<long> CreateOrderAsync(
            string fullName,
            string email,
            string address,
            string city,
            string state,
            string zipCode,
            List<OrderItemDto> items,
            string paymentMethod);
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}