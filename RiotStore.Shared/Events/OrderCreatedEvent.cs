using System;
using System.Collections.Generic;

namespace RiotStore.Shared.Events
{
    public class OrderCreatedEvent
    {
        public long OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public string ProductCategory { get; set; } = null!;
        public string CustomerSegment { get; set; } = "mid-demand";
        public bool IsRetry { get; set; } = false;
        public long? OriginalOrderId { get; set; }
        public int TotalQuantityRequested
        {
            get => Items?.Sum(i => i.Quantity) ?? 0;
        }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}