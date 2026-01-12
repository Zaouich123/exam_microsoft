using ECommerce.Api.Models;

namespace ECommerce.Api.Services;

public sealed class OrderResult
{
    private OrderResult(bool success, OrderResponse? response, List<string> errors)
    {
        Success = success;
        Response = response;
        Errors = errors;
    }

    public bool Success { get; }
    public OrderResponse? Response { get; }
    public List<string> Errors { get; }

    public static OrderResult Failure(List<string> errors) => new(false, null, errors);

    public static OrderResult SuccessResult(OrderResponse response) => new(true, response, new List<string>());
}
