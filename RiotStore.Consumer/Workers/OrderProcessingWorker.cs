using RiotStore.Consumer.Services.Interfaces;

namespace RiotStore.Consumer.Workers
{
    public class OrderProcessingWorker : BackgroundService
    {
        private readonly IKafkaConsumerService _kafkaConsumerService;

        public OrderProcessingWorker(IKafkaConsumerService kafkaConsumerService)
        {
            _kafkaConsumerService = kafkaConsumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _kafkaConsumerService.StartConsumingAsync(stoppingToken);
        }
    }
}