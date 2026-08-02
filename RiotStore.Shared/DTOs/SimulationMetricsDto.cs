namespace RiotStore.Shared.Dtos
{
    public class SimulationMetricsDto
    {
        public int TotalRequests { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}