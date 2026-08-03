using Confluent.Kafka;
using RiotStore.API.Services.Interfaces;
using RiotStore.Shared.Events;
using System.Text.Json;

namespace RiotStore.API.Services.Implementations
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendOrderCreatedEventAsync(OrderCreatedEvent orderEvent)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = orderEvent.OrderId.ToString(),
                    Value = JsonSerializer.Serialize(orderEvent)
                };

                var result = await _producer.ProduceAsync("order-events", message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error enviando evento a Kafka: {ex.Message}");
                throw;
            }
        }
    }
}