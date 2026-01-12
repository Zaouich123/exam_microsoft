using ECommerce.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Services;

public sealed class DbPromoCodeProvider : IPromoCodeProvider
{
    private readonly ECommerceDbContext _dbContext;

    public DbPromoCodeProvider(ECommerceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool TryGetDiscount(string promoCode, out decimal percentage)
    {
        percentage = 0m;
        var code = promoCode.Trim();
        var promo = _dbContext.PromoCodes.AsNoTracking()
            .FirstOrDefault(item => item.Code == code);
        if (promo is null)
        {
            return false;
        }

        percentage = promo.Percentage;
        return true;
    }
}
