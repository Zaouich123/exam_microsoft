namespace ECommerce.Api.Models;

public sealed class PromoCode
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
}
