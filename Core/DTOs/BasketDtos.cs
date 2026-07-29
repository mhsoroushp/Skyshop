namespace Core.DTOs;

public sealed class AddToBasketRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}


public sealed class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}

public class BasketQuantityItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class BasketItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageBase64 { get; set; } = string.Empty;
}