using System.Text.Json.Serialization;

namespace RiotStore.API.DTOs
{
    public class CustomerDto
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("address")]
        public string Address { get; set; } = null!;

        [JsonPropertyName("city")]
        public string City { get; set; } = null!;

        [JsonPropertyName("state")]
        public string State { get; set; } = null!;

        [JsonPropertyName("zipCode")]
        public string ZipCode { get; set; } = null!;
    }

    public class CartItemDto
    {
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }
    }

    public class CheckoutRequestDto
    {
        [JsonPropertyName("customer")]
        public CustomerDto Customer { get; set; } = null!;

        [JsonPropertyName("items")]
        public List<CartItemDto> Items { get; set; } = null!;

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = null!;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}