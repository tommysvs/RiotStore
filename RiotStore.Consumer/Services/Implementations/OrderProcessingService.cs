using RiotStore.Consumer.Services.Interfaces;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.Shared.Events;

namespace RiotStore.Consumer.Services.Implementations
{
    public class OrderProcessingService : IOrderProcessingService
    {
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(
            IStockRepository stockRepository,
            ILogger<OrderProcessingService> logger)
        {
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task ProcessOrderAsync(OrderCreatedEvent orderEvent)
        {
            try
            {
                if (orderEvent == null) return;

                foreach (var item in orderEvent.Items)
                {
                    var stockBalance = await _stockRepository.GetByProductIdAsync(item.ProductId);

                    int initialStock = stockBalance?.initial_stock ?? 0;
                    int newTotalAttempts = (stockBalance?.total_attempts ?? 0) + item.Quantity;
                    int newCurrentBalance = initialStock - newTotalAttempts;

                    await _stockRepository.UpdateOrCreateAsync(
                        item.ProductId,
                        initialStock,
                        newTotalAttempts,
                        newCurrentBalance);
                }

                _logger.LogInformation($"Orden {orderEvent.OrderId} procesada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error procesando orden: {ex.Message}");
                throw;
            }
        }
    }
}