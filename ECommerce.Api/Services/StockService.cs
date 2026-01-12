using ECommerce.Api.Models;

namespace ECommerce.Api.Services;

public sealed class StockService : IStockService
{
    private readonly List<Product> _products =
    [
        new Product { Id = 1, Name = "Produit A", Price = 10m, Stock = 20 },
        new Product { Id = 2, Name = "Produit B", Price = 15m, Stock = 10 },
        new Product { Id = 3, Name = "Produit C", Price = 8m, Stock = 5 },
        new Product { Id = 4, Name = "Produit D", Price = 25m, Stock = 7 },
        new Product { Id = 5, Name = "Produit E", Price = 50m, Stock = 3 }
    ];

    private readonly object _lock = new();

    public IReadOnlyList<Product> GetProducts()
    {
        lock (_lock)
        {
            return _products
                .Select(product => new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Stock = product.Stock
                })
                .ToList();
        }
    }

    public bool TryReserve(IReadOnlyList<OrderItemRequest> items, out List<string> errors)
    {
        errors = new List<string>();
        lock (_lock)
        {
            foreach (var item in items)
            {
                var product = _products.FirstOrDefault(p => p.Id == item.Id);
                if (product is null)
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
                var product = _products.First(p => p.Id == item.Id);
                product.Stock -= item.Quantity;
            }

            return true;
        }
    }
}
