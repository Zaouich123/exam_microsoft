namespace ECommerce.Api.Models;

public sealed class ErrorResponse
{
    public List<string> Errors { get; init; } = new();
}
