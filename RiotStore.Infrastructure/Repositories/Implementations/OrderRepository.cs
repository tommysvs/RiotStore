using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;

namespace RiotStore.Infrastructure.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly RiotStoreDbContext _context;

        public OrderRepository(RiotStoreDbContext context)
        {
            _context = context;
        }
    }
}