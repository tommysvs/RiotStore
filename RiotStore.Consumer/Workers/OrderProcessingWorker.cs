using RiotStore.Consumer.Services.Interfaces;

namespace RiotStore.Consumer.Workers
{
    public class OrderProcessingWorker : BackgroundService
    {
        private readonly IKafkaConsumerService _kafkaConsumerService;
        private readonly ILogger<OrderProcessingWorker> _logger;

        public OrderProcessingWorker(
            IKafkaConsumerService kafkaConsumerService,
            ILogger<OrderProcessingWorker> logger)
        {
            _kafkaConsumerService = kafkaConsumerService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderProcessingWorker iniciado");
            await _kafkaConsumerService.StartConsumingAsync(stoppingToken);
        }
    }
}