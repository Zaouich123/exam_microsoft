using System.Text.Json.Serialization;

namespace ECommerce.Api.Models;

public sealed class OrderProductResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }

    [JsonPropertyName("price_per_unit")]
    public decimal PricePerUnit { get; init; }
    public decimal Total { get; init; }
}
