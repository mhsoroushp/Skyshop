namespace API.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ProductName { get; set; }
}

public class CreateOrderRequest
{
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
}
