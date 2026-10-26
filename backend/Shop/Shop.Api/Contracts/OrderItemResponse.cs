namespace Shop.Api.Contracts;

public record OrderItemResponse(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);