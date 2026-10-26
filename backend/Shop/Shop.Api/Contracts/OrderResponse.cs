namespace Shop.Api.Contracts;

public record OrderResponse(
    Guid Id,
    long Number,
    string DeliveryAddress,
    DateTime CreatedAt,
    List<OrderItemResponse> Items,
    decimal Total);