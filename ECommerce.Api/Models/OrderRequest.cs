using System.Text.Json.Serialization;

namespace ECommerce.Api.Models;

public sealed class OrderRequest
{
    public List<OrderItemRequest> Products { get; init; } = new();

    [JsonPropertyName("promo_code")]
    public string? PromoCode { get; init; }
}
