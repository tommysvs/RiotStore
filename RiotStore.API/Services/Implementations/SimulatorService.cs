using RiotStore.API.Services.Interfaces;
using RiotStore.Shared.Events;

namespace RiotStore.API.Services.Implementations
{
    public class SimulatorService : ISimulatorService
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly IProductService _productService;
        private readonly ILogger<SimulatorService> _logger;

        public SimulatorService(IKafkaProducerService kafkaProducer, IProductService productService, ILogger<SimulatorService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _productService = productService;
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

        public async Task<SimulationMetricsDto> SimulateBatchWithMetricsAsync(int productId, int quantity, int batchCount)
        {
            var metrics = new SimulationMetricsDto
            {
                TotalRequests = quantity * batchCount,
                SuccessCount = 0,
                FailureCount = 0,
                StartedAt = DateTime.UtcNow
            };

            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    metrics.FailureCount = metrics.TotalRequests;
                    _logger.LogWarning($"Producto {productId} no encontrado para simulación");
                    return metrics;
                }

                var tasks = new List<Task>();
                for (int i = 0; i < batchCount; i++)
                {
                    for (int j = 0; j < quantity; j++)
                    {
                        tasks.Add(SimulatePurchaseAttemptAsync(productId, product.Name, 1));
                    }
                }

                await Task.WhenAll(tasks);
                metrics.SuccessCount = metrics.TotalRequests;
                _logger.LogInformation($"Simulación de lote completada: {metrics.TotalRequests} peticiones exitosas");
            }
            catch (Exception ex)
            {
                metrics.FailureCount = metrics.TotalRequests;
                _logger.LogError($"Error en simulación de lote: {ex.Message}");
            }

            metrics.CompletedAt = DateTime.UtcNow;
            return metrics;
        }
    }
}