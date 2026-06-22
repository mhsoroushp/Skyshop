namespace Core.Interfaces;
public interface ICartRepository
{
    Task AddToBasket(string basketKey, Guid productId, int quantity);
    Task<BasketQuantityItem[]> GetBasket(string basketKey);
    Task RemoveFromBasket(string basketKey, Guid productId);
    Task UpdateQuantity(string basketKey, Guid productId, int quantity);
    Task ClearBasket(string basketKey);
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
    public string? ProductName { get; set; }
}