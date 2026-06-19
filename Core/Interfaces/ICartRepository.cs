namespace Core.Interfaces;
public interface ICartRepository
{
    Task AddToBasket(int productId, int quantity);
    Task<BasketItem[]> GetBasket();
    Task RemoveFromBasket(int productId);
    Task UpdateQuantity(int productId, int quantity);
    Task ClearBasket();
    Task<int> GetBasketItemCount();
    Task<decimal> GetBasketTotalPrice();
}

public class BasketItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ProductName { get; set; }
}