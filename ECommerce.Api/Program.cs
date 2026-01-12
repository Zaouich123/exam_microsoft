using ECommerce.Api.Data;
using ECommerce.Api.Models;
using ECommerce.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseInMemoryDatabase("ECommerceDb"));
builder.Services.AddScoped<IStockService, DbStockService>();
builder.Services.AddScoped<IPromoCodeProvider, DbPromoCodeProvider>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    if (!dbContext.Products.Any())
    {
        dbContext.Products.AddRange(
            new Product { Id = 1, Name = "Produit A", Price = 10m, Stock = 20 },
            new Product { Id = 2, Name = "Produit B", Price = 15m, Stock = 10 },
            new Product { Id = 3, Name = "Produit C", Price = 8m, Stock = 5 },
            new Product { Id = 4, Name = "Produit D", Price = 25m, Stock = 7 },
            new Product { Id = 5, Name = "Produit E", Price = 50m, Stock = 3 }
        );
        dbContext.PromoCodes.AddRange(
            new PromoCode { Id = 1, Code = "DISCOUNT20", Percentage = 20m },
            new PromoCode { Id = 2, Code = "DISCOUNT 10", Percentage = 10m }
        );
        dbContext.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/products", (IStockService stockService) =>
{
    return stockService.GetProducts();
});

app.MapPost("/orders", (OrderRequest request, OrderService orderService) =>
{
    var result = orderService.CreateOrder(request);
    if (!result.Success)
    {
        return Results.BadRequest(new ErrorResponse { Errors = result.Errors });
    }

    return Results.Ok(result.Response);
});

app.Run();
