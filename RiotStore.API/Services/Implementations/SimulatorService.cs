using RiotStore.API.Services.Interfaces;
using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.Shared.Events;
using RiotStore.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace RiotStore.API.Services.Implementations
{
    public class SimulatorService : ISimulatorService
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly IProductRepository _productRepository;
        private readonly IDataGeneratorService _dataGenerator;
        private readonly RiotStoreDbContext _context;
        private readonly ILogger<SimulatorService> _logger;

        public SimulatorService(
            IKafkaProducerService kafkaProducer,
            IProductRepository productRepository,
            IDataGeneratorService dataGenerator,
            RiotStoreDbContext context,
            ILogger<SimulatorService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _productRepository = productRepository;
            _dataGenerator = dataGenerator;
            _context = context;
            _logger = logger;
        }

        public async Task SimulatePurchaseAttemptAsync(int productId, string productName, int quantity)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            var orderEvent = new OrderCreatedEvent
            {
                OrderId = GenerateUniqueOrderId(),
                CustomerId = new Random().Next(1, 1000),
                CreatedAt = DateTime.UtcNow,
                TotalAmount = quantity * product.price,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductId = productId,
                        ProductName = productName,
                        Quantity = quantity,
                        UnitPrice = product.price
                    }
                }
            };

            await _kafkaProducer.SendOrderCreatedEventAsync(orderEvent);
        }

        public async Task SimulateBatchPurchaseAsync(List<(int productId, string productName, int quantity)> purchases)
        {
            foreach (var (productId, productName, quantity) in purchases)
            {
                await SimulatePurchaseAttemptAsync(productId, productName, quantity);
            }
        }

        public async Task<SimulationMetricsDto> SimulateBatchWithMetricsAsync(int quantity, int batchCount)
        {
            var startTime = DateTime.UtcNow;

            var metrics = new SimulationMetricsDto
            {
                TotalRequests = quantity * batchCount,
                SuccessCount = 0,
                FailureCount = 0,
                StartedAt = startTime
            };

            try
            {
                var events = await _dataGenerator.GenerateBatchAsync(metrics.TotalRequests);

                foreach (var @event in events)
                {
                    try
                    {
                        await _kafkaProducer.SendOrderCreatedEventAsync(@event);
                        metrics.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        metrics.FailureCount++;
                        _logger.LogError($"Error enviando evento: {ex.Message}");
                    }
                }

                _logger.LogInformation($"Simulación completada: {metrics.SuccessCount} exitosos, {metrics.FailureCount} fallidos");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en simulación batch: {ex.Message}\n{ex.StackTrace}");
                metrics.FailureCount = metrics.TotalRequests;
            }

            metrics.CompletedAt = DateTime.UtcNow;

            var elapsedSeconds = (metrics.CompletedAt - startTime).TotalSeconds;
            var eventsPerSecond = elapsedSeconds > 0 ? metrics.SuccessCount / elapsedSeconds : 0;

            try
            {
                var benchmark = new GeneratorBenchmark
                {
                    total_events_generated = metrics.SuccessCount,
                    elapsed_seconds = elapsedSeconds,
                    events_per_second = eventsPerSecond,
                    measured_at = DateTime.UtcNow,
                    notes = $"Batch simulation: {batchCount} batches × {quantity} items"
                };

                _context.GeneratorBenchmarks.Add(benchmark);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Benchmark guardado: {metrics.SuccessCount} eventos en {elapsedSeconds:F2}s ({eventsPerSecond:F2} evt/s)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error guardando benchmark: {ex.Message}\n{ex.StackTrace}");
            }

            return metrics;
        }

        private long GenerateUniqueOrderId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}