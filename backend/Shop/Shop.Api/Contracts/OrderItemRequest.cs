using System.ComponentModel.DataAnnotations;

namespace Shop.Api.Contracts;

public record OrderItemRequest(
    Guid ProductId,
    [Range(1, 500)] int Quantity);