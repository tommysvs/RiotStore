using Confluent.Kafka;
using RiotStore.Consumer.Services.Interfaces;
using RiotStore.Shared.Events;
using System.Text.Json;

namespace RiotStore.Consumer.Services.Implementations
{
    public class KafkaConsumerService : IKafkaConsumerService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<KafkaConsumerService> _logger;

        public KafkaConsumerService(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<KafkaConsumerService> logger)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
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
                            
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var orderProcessingService = scope.ServiceProvider.GetRequiredService<IOrderProcessingService>();
                                await orderProcessingService.ProcessOrderAsync(orderEvent);
                            }
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