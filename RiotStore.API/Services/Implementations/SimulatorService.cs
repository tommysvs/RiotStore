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
                ProductCategory = product.category?.name ?? "Sin categoría",
                CustomerSegment = "normal-user",
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
            var totalEvents = quantity * batchCount;
            int sentCount = 0;

            try
            {
                var events = await _dataGenerator.GenerateBatchAsync(totalEvents);

                foreach (var @event in events)
                {
                    try
                    {
                        await _kafkaProducer.SendOrderCreatedEventAsync(@event);
                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error enviando evento a Kafka: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en simulación batch: {ex.Message}");
            }

            var completedAt = DateTime.UtcNow;
            var elapsedSeconds = (completedAt - startTime).TotalSeconds;
            var eventsPerSecond = elapsedSeconds > 0 ? sentCount / elapsedSeconds : 0;

            await SaveBenchmarkAsync(sentCount, elapsedSeconds, eventsPerSecond, quantity, batchCount);

            return new SimulationMetricsDto
            {
                TotalRequests = totalEvents,
                SuccessCount = sentCount,
                FailureCount = totalEvents - sentCount,
                StartedAt = startTime,
                CompletedAt = completedAt
            };
        }

        private async Task SaveBenchmarkAsync(int eventsGenerated, double elapsedSeconds, double eventsPerSecond, int quantity, int batchCount)
        {
            try
            {
                var benchmark = new GeneratorBenchmark
                {
                    total_events_generated = eventsGenerated,
                    elapsed_seconds = elapsedSeconds,
                    events_per_second = eventsPerSecond,
                    measured_at = DateTime.UtcNow,
                    notes = $"Simulación: {batchCount} lotes × {quantity} eventos"
                };

                _context.GeneratorBenchmarks.Add(benchmark);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error guardando benchmark: {ex.Message}");
            }
        }

        private long GenerateUniqueOrderId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}