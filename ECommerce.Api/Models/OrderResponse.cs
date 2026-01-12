namespace ECommerce.Api.Models;

public sealed class OrderResponse
{
    public List<OrderProductResponse> Products { get; init; } = new();
    public List<DiscountResponse> Discounts { get; init; } = new();
    public decimal Total { get; init; }
}
