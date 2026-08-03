using RiotStore.API.Services.Interfaces;
using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Interfaces;
using RiotStore.Shared.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RiotStore.API.Services.Implementations
{
    public class DataGeneratorService : IDataGeneratorService
    {
        private readonly IProductRepository _productRepository;
        private readonly Random _random = new();

        private List<Product>? _productsCache = null;
        private DateTime _productsCacheTime = DateTime.MinValue;
        private const int CACHE_DURATION_SECONDS = 60;

        private static readonly Dictionary<string, decimal> CategoryDemandWeights = new()
        {
            { "Estatuas y Figuras", 0.30m },
            { "Coleccionables", 0.35m },
            { "Ropa", 0.25m },
            { "Peluches", 0.10m }
        };

        private static readonly Dictionary<string, decimal> CustomerSegmentDistribution = new()
        {
            { "high-demand", 0.20m },
            { "mid-demand", 0.50m },
            { "low-demand", 0.30m }
        };

        public DataGeneratorService(
            IProductRepository productRepository,
            ILogger<DataGeneratorService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        private async Task<List<Product>> GetProductsAsync()
        {
            if (_productsCache != null && (DateTime.UtcNow - _productsCacheTime).TotalSeconds < CACHE_DURATION_SECONDS)
            {
                return _productsCache;
            }

            _productsCache = await _productRepository.GetAllProductsAsync();
            _productsCacheTime = DateTime.UtcNow;

            if (_productsCache == null || _productsCache.Count == 0)
            {
                throw new InvalidOperationException("No products available for generation");
            }

            return _productsCache;
        }

        public async Task<OrderCreatedEvent> GenerateSinglePurchaseAttemptAsync()
        {
            var products = await GetProductsAsync();

            var selectedProduct = SelectProductByDemand(products);
            var customerSegment = SelectCustomerSegment();
            var quantity = GenerateQuantity(customerSegment);

            var orderId = GenerateUniqueOrderId();
            var categoryName = selectedProduct.category?.name ?? "Sin categoría";

            var orderEvent = new OrderCreatedEvent
            {
                OrderId = orderId,
                CustomerId = GenerateCustomerId(customerSegment),
                CreatedAt = DateTime.UtcNow,
                TotalAmount = selectedProduct.price * quantity,
                ProductCategory = categoryName,
                CustomerSegment = customerSegment,
                Items = new()
                {
                    new OrderItemDto
                    {
                        ProductId = selectedProduct.product_id,
                        ProductName = selectedProduct.name,
                        Quantity = quantity,
                        UnitPrice = selectedProduct.price
                    }
                }
            };

            return orderEvent;
        }

        public async Task<List<OrderCreatedEvent>> GenerateBatchAsync(
            int count,
            string? targetProductCategory = null,
            bool simulatePeakHour = false)
        {
            var products = await GetProductsAsync();
            var peakHourMultiplier = simulatePeakHour ? 1.5m : 1.0m;
            var events = new List<OrderCreatedEvent>(count);
            int skippedCount = 0;

            for (int i = 0; i < count; i++)
            {
                var selectedProduct = SelectProductByDemand(products);
                var customerSegment = SelectCustomerSegment();
                var quantity = GenerateQuantity(customerSegment);

                if (!string.IsNullOrEmpty(targetProductCategory) &&
                    !selectedProduct.category?.name?.Equals(targetProductCategory, StringComparison.OrdinalIgnoreCase) == true)
                {
                    i--;
                    skippedCount++;
                    
                    if (skippedCount > count * 2)
                    {
                        break;
                    }
                    continue;
                }

                var orderId = GenerateUniqueOrderId();
                var categoryName = selectedProduct.category?.name ?? "Sin categoría";

                var orderEvent = new OrderCreatedEvent
                {
                    OrderId = orderId,
                    CustomerId = GenerateCustomerId(customerSegment),
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = selectedProduct.price * quantity,
                    ProductCategory = categoryName,
                    CustomerSegment = customerSegment,
                    Items = new()
                    {
                        new OrderItemDto
                        {
                            ProductId = selectedProduct.product_id,
                            ProductName = selectedProduct.name,
                            Quantity = (int)(quantity * (decimal)peakHourMultiplier),
                            UnitPrice = selectedProduct.price
                        }
                    }
                };

                events.Add(orderEvent);
                skippedCount = 0;
            }

            return events;
        }

        private Product SelectProductByDemand(List<Product> products)
        {
            var byCategory = products
                .GroupBy(p => p.category?.name ?? "Sin categoría")
                .ToDictionary(g => g.Key, g => g.ToList());

            var selectedCategory = SelectByWeight(
                byCategory.Keys.ToList(),
                CategoryDemandWeights
            );

            var productsInCategory = byCategory[selectedCategory];
            return productsInCategory[_random.Next(productsInCategory.Count)];
        }

        private string SelectCustomerSegment()
        {
            return SelectByWeight(
                CustomerSegmentDistribution.Keys.ToList(),
                CustomerSegmentDistribution
            );
        }

        private string SelectByWeight(List<string> items, Dictionary<string, decimal> weights)
        {
            if (items.Count == 0)
                return "mid-demand";

            var randomValue = (decimal)_random.NextDouble();
            var cumulative = 0m;

            foreach (var item in items)
            {
                if (weights.TryGetValue(item, out var weight))
                {
                    cumulative += weight;
                    if (randomValue <= cumulative)
                    {
                        return item;
                    }
                }
            }

            return items.Last();
        }

        private int GenerateQuantity(string customerSegment)
        {
            return customerSegment switch
            {
                "high-demand" => _random.Next(1, 6),
                "mid-demand" => _random.Next(1, 4),
                "low-demand" => _random.Next(1, 3),
                _ => 1
            };
        }

        private int GenerateCustomerId(string customerSegment)
        {
            return customerSegment switch
            {
                "high-demand" => _random.Next(1, 101),
                "mid-demand" => _random.Next(101, 1001),
                "low-demand" => _random.Next(1001, 10001),
                _ => _random.Next(1, 10001)
            };
        }

        private long GenerateUniqueOrderId()
        {
            return long.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
                   * 100000 + _random.Next(100000);
        }
    }
}