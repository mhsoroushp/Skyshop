using Core.DTOs;

namespace Core.Interfaces;

public interface IBasketService
{
	Task AddToBasket(AddToBasketRequest request);
	Task<List<BasketItem>> GetBasket();
	Task RemoveFromBasket(Guid productId);
	Task UpdateQuantity(Guid productId, int quantity);
	Task ClearBasket();
	Task<int> GetBasketItemCount();
	Task<decimal> GetBasketTotalPrice();
}
