using ECommerce.Api.Models;

namespace ECommerce.Api.Services;

public sealed class OrderService
{
    private readonly IStockService _stockService;
    private readonly IPromoCodeProvider _promoCodeProvider;

    public OrderService(IStockService stockService)
        : this(stockService, new InMemoryPromoCodeProvider())
    {
    }

    public OrderService(IStockService stockService, IPromoCodeProvider promoCodeProvider)
    {
        _stockService = stockService;
        _promoCodeProvider = promoCodeProvider;
    }

    public OrderResult CreateOrder(OrderRequest request)
    {
        var errors = new List<string>();
        var items = request.Products ?? new List<OrderItemRequest>();
        var catalog = _stockService.GetProducts().ToDictionary(product => product.Id);
        var hasProductErrors = false;

        foreach (var item in items)
        {
            if (!catalog.TryGetValue(item.Id, out var product))
            {
                errors.Add($"Le produit avec l’identifiant {item.Id} n’existe pas");
                hasProductErrors = true;
                continue;
            }

            if (item.Quantity > product.Stock)
            {
                errors.Add($"Il ne reste que {product.Stock} exemplaire pour le produit {product.Name}");
                hasProductErrors = true;
            }
        }

        var baseTotal = items
            .Where(item => catalog.ContainsKey(item.Id))
            .Sum(item => catalog[item.Id].Price * item.Quantity);

        var promoCode = request.PromoCode?.Trim();
        var promoDiscount = 0m;
        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            if (!_promoCodeProvider.TryGetDiscount(promoCode, out promoDiscount))
            {
                errors.Add("Le code promo est invalide");
            }
            else if (!hasProductErrors && baseTotal < 50m)
            {
                errors.Add("Les codes promos ne sont valables qu’à partir de 50 euros d’achat");
            }
        }

        if (errors.Count > 0)
        {
            return OrderResult.Failure(errors);
        }

        var responseProducts = new List<OrderProductResponse>();
        var discountedSubtotal = 0m;

        foreach (var item in items)
        {
            var product = catalog[item.Id];
            var lineTotal = product.Price * item.Quantity;
            if (item.Quantity > 5)
            {
                lineTotal *= 0.9m;
            }

            lineTotal = RoundCurrency(lineTotal);
            discountedSubtotal += lineTotal;

            responseProducts.Add(new OrderProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = item.Quantity,
                PricePerUnit = product.Price,
                Total = lineTotal
            });
        }

        var discounts = new List<DiscountResponse>();
        var orderDiscount = discountedSubtotal > 100m ? 5m : 0m;
        if (orderDiscount > 0m)
        {
            discounts.Add(new DiscountResponse { Type = "order", Value = orderDiscount });
        }

        if (promoDiscount > 0m)
        {
            discounts.Add(new DiscountResponse { Type = "promo", Value = promoDiscount });
        }

        var total = discountedSubtotal * (100m - orderDiscount - promoDiscount) / 100m;
        total = RoundCurrency(total);

        if (!_stockService.TryReserve(items, out var reserveErrors))
        {
            return OrderResult.Failure(reserveErrors);
        }

        var response = new OrderResponse
        {
            Products = responseProducts,
            Discounts = discounts,
            Total = total
        };

        return OrderResult.SuccessResult(response);
    }

    private static decimal RoundCurrency(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
