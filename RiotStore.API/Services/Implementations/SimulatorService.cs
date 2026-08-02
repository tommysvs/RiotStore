using RiotStore.API.Services.Interfaces;
using RiotStore.Shared.Events;

namespace RiotStore.API.Services.Implementations
{
    public class SimulatorService : ISimulatorService
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<SimulatorService> _logger;

        public SimulatorService(IKafkaProducerService kafkaProducer, ILogger<SimulatorService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task SimulatePurchaseAttemptAsync(int productId, string productName, int quantity)
        {
            var orderEvent = new OrderCreatedEvent
            {
                OrderId = new Random().Next(10000, 99999),
                CustomerId = new Random().Next(1, 1000),
                CreatedAt = DateTime.UtcNow,
                TotalAmount = quantity * 29.99m,
                Items = new()
                {
                    new OrderItemDto
                    {
                        ProductId = productId,
                        ProductName = productName,
                        Quantity = quantity,
                        UnitPrice = 29.99m
                    }
                }
            };

            await _kafkaProducer.SendOrderCreatedEventAsync(orderEvent);
            _logger.LogInformation($"Intento de compra simulado: Producto {productId}, Cantidad: {quantity}");
        }

        public async Task SimulateBatchPurchaseAsync(List<(int productId, string productName, int quantity)> purchases)
        {
            var tasks = purchases.Select(p => SimulatePurchaseAttemptAsync(p.productId, p.productName, p.quantity));
            await Task.WhenAll(tasks);
            _logger.LogInformation($"Lote de {purchases.Count} intentos de compra simulados");
        }
    }
}