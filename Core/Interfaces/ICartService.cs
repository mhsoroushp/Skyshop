namespace Core.Interfaces;

public interface ICartService
{
	Task AddToBasket(Guid productId, int quantity);
	Task<BasketItem[]> GetBasket();
	Task RemoveFromBasket(Guid productId);
	Task UpdateQuantity(Guid productId, int quantity);
	Task ClearBasket();
	Task<int> GetBasketItemCount();
	Task<decimal> GetBasketTotalPrice();
}
