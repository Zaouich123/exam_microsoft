namespace ECommerce.Api.Services;

public sealed class InMemoryPromoCodeProvider : IPromoCodeProvider
{
    private readonly Dictionary<string, decimal> _promoCodes = new(StringComparer.Ordinal)
    {
        ["DISCOUNT20"] = 20m,
        ["DISCOUNT 10"] = 10m
    };

    public bool TryGetDiscount(string promoCode, out decimal percentage) =>
        _promoCodes.TryGetValue(promoCode.Trim(), out percentage);
}
