using System;

namespace RiotStore.Shared.Events
{
    public class StockUpdatedEvent
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}