using RiotStore.Shared.Events;

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
}