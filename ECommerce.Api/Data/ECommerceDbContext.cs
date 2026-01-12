using ECommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Data;

public sealed class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
}
