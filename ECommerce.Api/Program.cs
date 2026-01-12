using ECommerce.Api.Models;
using ECommerce.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IStockService, StockService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

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
