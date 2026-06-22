namespace Core.Interfaces;
public interface ICartRepository
{
    Task AddToBasket(Guid productId, int quantity);
    Task<BasketItem[]> GetBasket();
    Task RemoveFromBasket(Guid productId);
    Task UpdateQuantity(Guid productId, int quantity);
    Task ClearBasket();
    Task<int> GetBasketItemCount();
    Task<decimal> GetBasketTotalPrice();
}

public class BasketItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ProductName { get; set; }
}