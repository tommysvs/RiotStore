using Confluent.Kafka;
using RiotStore.Consumer.Services.Interfaces;
using RiotStore.Shared.Events;
using System.Text.Json;

namespace RiotStore.Consumer.Services.Implementations
{
    public class KafkaConsumerService : IKafkaConsumerService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger<KafkaConsumerService> _logger;

        public KafkaConsumerService(
            IConfiguration configuration,
            IOrderProcessingService orderProcessingService,
            ILogger<KafkaConsumerService> logger)
        {
            _configuration = configuration;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "riotstore-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using (var consumer = new ConsumerBuilder<string, string>(config).Build())
            {
                consumer.Subscribe("order-events");
                _logger.LogInformation("Consumidor suscrito a topic: order-events");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var message = consumer.Consume(cancellationToken);
                        if (message != null)
                        {
                            _logger.LogInformation($"Evento recibido: {message.Value}");
                            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Value);
                            await _orderProcessingService.ProcessOrderAsync(orderEvent);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error procesando evento: {ex.Message}");
                    }
                }
            }
        }
    }
}