namespace Core.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string SessionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public List<OrderItem> Items { get; set; } = new();
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
