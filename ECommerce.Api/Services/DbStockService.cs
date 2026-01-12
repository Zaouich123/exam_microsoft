using ECommerce.Api.Data;
using ECommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Services;

public sealed class DbStockService : IStockService
{
    private readonly ECommerceDbContext _dbContext;

    public DbStockService(ECommerceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<Product> GetProducts()
    {
        return _dbContext.Products.AsNoTracking()
            .Select(product => new Product
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock
            })
            .ToList();
    }

    public bool TryReserve(IReadOnlyList<OrderItemRequest> items, out List<string> errors)
    {
        errors = new List<string>();
        var products = _dbContext.Products.ToDictionary(product => product.Id);

        foreach (var item in items)
        {
            if (!products.TryGetValue(item.Id, out var product))
            {
                errors.Add($"Le produit avec l’identifiant {item.Id} n’existe pas");
                continue;
            }

            if (item.Quantity > product.Stock)
            {
                errors.Add($"Il ne reste que {product.Stock} exemplaire pour le produit {product.Name}");
            }
        }

        if (errors.Count > 0)
        {
            return false;
        }

        foreach (var item in items)
        {
            products[item.Id].Stock -= item.Quantity;
        }

        _dbContext.SaveChanges();
        return true;
    }
}
