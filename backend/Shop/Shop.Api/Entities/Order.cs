namespace Shop.Api.Entities;

public class Order
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}