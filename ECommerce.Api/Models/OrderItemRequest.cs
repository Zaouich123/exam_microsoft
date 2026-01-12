namespace ECommerce.Api.Models;

public sealed class OrderItemRequest
{
    public int Id { get; init; }
    public int Quantity { get; init; }
}
