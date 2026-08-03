namespace RiotStore.API.Dtos
{
    public class DashboardStatisticsDto
    {
        public int TotalInventory { get; set; }
        public int TotalAttempts { get; set; }
        public int TotalBalance { get; set; }
        public int TotalSold { get; set; }
        public int OversoldProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int AvailableProducts { get; set; }
        public int ExhaustedProducts { get; set; }
        public double GlobalConversionRate { get; set; }
        public double GlobalOverselling { get; set; }
        public DateTime Timestamp { get; set; }
    }
}