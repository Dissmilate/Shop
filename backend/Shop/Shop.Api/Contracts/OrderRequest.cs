using System.ComponentModel.DataAnnotations;

namespace Shop.Api.Contracts;

public record OrderRequest(
    [Required, StringLength(500)] string DeliveryAddress,
    [Required, MinLength(1)] List<OrderItemRequest> Items);