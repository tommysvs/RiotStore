namespace RiotStore.API.Dtos
{
    public class StockBalanceDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductSku { get; set; } = null!;
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "Sin categoría";
        public int InitialStock { get; set; }
        public int CurrentBalance { get; set; }
        public int TotalAttempts { get; set; }
        public int SoldUnits { get; set; }
        public double ConversionRate { get; set; }
        public double OversellingPercentage { get; set; }
        public string Status { get; set; } = null!;
        public DateTime LastUpdated { get; set; }
        public bool IsOversold { get; set; }
        public double PercentageRemaining { get; set; }
    }
}