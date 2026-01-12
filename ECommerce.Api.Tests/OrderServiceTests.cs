using ECommerce.Api.Models;
using ECommerce.Api.Services;
using Xunit;

namespace ECommerce.Api.Tests;

public class OrderServiceTests
{
    [Fact]
    public void CreateOrder_Success_NoDiscounts()
    {
        var stockService = new StockService();
        var service = new OrderService(stockService);
        var request = new OrderRequest
        {
            Products =
            [
                new OrderItemRequest { Id = 1, Quantity = 2 },
                new OrderItemRequest { Id = 2, Quantity = 1 }
            ]
        };

        var result = service.CreateOrder(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Response);
        Assert.Equal(35m, result.Response!.Total);
        Assert.Empty(result.Response.Discounts);
        Assert.Collection(result.Response.Products,
            first =>
            {
                Assert.Equal(1, first.Id);
                Assert.Equal(2, first.Quantity);
                Assert.Equal(10m, first.PricePerUnit);
                Assert.Equal(20m, first.Total);
            },
            second =>
            {
                Assert.Equal(2, second.Id);
                Assert.Equal(1, second.Quantity);
                Assert.Equal(15m, second.PricePerUnit);
                Assert.Equal(15m, second.Total);
            });

        var updatedProduct = stockService.GetProducts().First(p => p.Id == 1);
        Assert.Equal(18, updatedProduct.Stock);
    }

    [Fact]
    public void CreateOrder_AppliesProductDiscount_WhenQuantityGreaterThanFive()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products = [new OrderItemRequest { Id = 1, Quantity = 6 }]
        };

        var result = service.CreateOrder(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Response);
        Assert.Equal(54m, result.Response!.Total);
        Assert.Empty(result.Response.Discounts);
        Assert.Single(result.Response.Products);
        Assert.Equal(54m, result.Response.Products[0].Total);
    }

    [Fact]
    public void CreateOrder_AppliesOrderDiscount_WhenTotalExceeds100()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products = [new OrderItemRequest { Id = 4, Quantity = 5 }]
        };

        var result = service.CreateOrder(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Response);
        Assert.Equal(118.75m, result.Response!.Total);
        Assert.Single(result.Response.Discounts);
        Assert.Equal("order", result.Response.Discounts[0].Type);
        Assert.Equal(5m, result.Response.Discounts[0].Value);
    }

    [Fact]
    public void CreateOrder_AppliesPromoDiscount_WhenValidAndEligible()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products =
            [
                new OrderItemRequest { Id = 4, Quantity = 2 },
                new OrderItemRequest { Id = 5, Quantity = 1 }
            ],
            PromoCode = "DISCOUNT20"
        };

        var result = service.CreateOrder(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Response);
        Assert.Equal(80m, result.Response!.Total);
        Assert.Single(result.Response.Discounts);
        Assert.Equal("promo", result.Response.Discounts[0].Type);
        Assert.Equal(20m, result.Response.Discounts[0].Value);
    }

    [Fact]
    public void CreateOrder_AppliesOrderAndPromoDiscounts_Additive()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products = [new OrderItemRequest { Id = 4, Quantity = 5 }],
            PromoCode = "DISCOUNT 10"
        };

        var result = service.CreateOrder(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Response);
        Assert.Equal(106.25m, result.Response!.Total);
        Assert.Equal(2, result.Response.Discounts.Count);
        Assert.Contains(result.Response.Discounts, d => d.Type == "order" && d.Value == 5m);
        Assert.Contains(result.Response.Discounts, d => d.Type == "promo" && d.Value == 10m);
    }

    [Fact]
    public void CreateOrder_ReturnsError_WhenPromoInvalid()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products = [new OrderItemRequest { Id = 1, Quantity = 1 }],
            PromoCode = "BAD"
        };

        var result = service.CreateOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Le code promo est invalide", result.Errors);
    }

    [Fact]
    public void CreateOrder_ReturnsError_WhenPromoNotEligible()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products = [new OrderItemRequest { Id = 1, Quantity = 2 }],
            PromoCode = "DISCOUNT20"
        };

        var result = service.CreateOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Les codes promos ne sont valables qu’à partir de 50 euros d’achat", result.Errors);
    }

    [Fact]
    public void CreateOrder_ReturnsError_WhenProductMissing()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products = [new OrderItemRequest { Id = 999, Quantity = 1 }]
        };

        var result = service.CreateOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Le produit avec l’identifiant 999 n’existe pas", result.Errors);
    }

    [Fact]
    public void CreateOrder_ReturnsError_WhenStockInsufficient()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products = [new OrderItemRequest { Id = 3, Quantity = 6 }]
        };

        var result = service.CreateOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Il ne reste que 5 exemplaire pour le produit Produit C", result.Errors);
    }

    [Fact]
    public void CreateOrder_ReturnsMultipleErrors()
    {
        var service = new OrderService(new StockService());
        var request = new OrderRequest
        {
            Products =
            [
                new OrderItemRequest { Id = 999, Quantity = 1 },
                new OrderItemRequest { Id = 3, Quantity = 6 }
            ],
            PromoCode = "BAD"
        };

        var result = service.CreateOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Le produit avec l’identifiant 999 n’existe pas", result.Errors);
        Assert.Contains("Il ne reste que 5 exemplaire pour le produit Produit C", result.Errors);
        Assert.Contains("Le code promo est invalide", result.Errors);
    }
}
