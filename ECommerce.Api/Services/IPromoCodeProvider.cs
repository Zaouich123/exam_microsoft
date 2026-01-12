namespace ECommerce.Api.Services;

public interface IPromoCodeProvider
{
    bool TryGetDiscount(string promoCode, out decimal percentage);
}
