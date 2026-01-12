using ECommerce.Api.Models;

namespace ECommerce.Api.Services;

public interface IStockService
{
    IReadOnlyList<Product> GetProducts();
    bool TryReserve(IReadOnlyList<OrderItemRequest> items, out List<string> errors);
}
