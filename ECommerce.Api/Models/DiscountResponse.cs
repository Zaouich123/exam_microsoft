namespace ECommerce.Api.Models;

public sealed class DiscountResponse
{
    public string Type { get; init; } = string.Empty;
    public decimal Value { get; init; }
}
