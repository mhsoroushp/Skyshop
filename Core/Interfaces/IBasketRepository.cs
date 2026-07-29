using Core.DTOs;

namespace Core.Interfaces;
public interface IBasketRepository
{
    Task AddToBasket(string basketKey, Guid productId, int quantity);
    Task<List<BasketQuantityItem>> GetBasket(string basketKey);
    Task RemoveFromBasket(string basketKey, Guid productId);
    Task UpdateQuantity(string basketKey, Guid productId, int quantity);
    Task ClearBasket(string basketKey);
}