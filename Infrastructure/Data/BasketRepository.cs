using Core.DTOs;
using Core.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Data;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _redis;

    public BasketRepository(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task AddToBasket(string basketKey, Guid productId, int quantity)
    {
        // Increment quantity (auto-creates key if doesn't exist)
        await _redis.HashIncrementAsync(basketKey, productId.ToString(), quantity);

        // Set 7-day TTL (resets on each update)
        await _redis.KeyExpireAsync(basketKey, TimeSpan.FromDays(7));
    }

    public async Task<List<BasketQuantityItem>> GetBasket(string basketKey)
    {
        // Get all products and quantities
        var hashEntries = await _redis.HashGetAllAsync(basketKey);

        if (hashEntries.Length == 0)
            return [];

        var basketItems = new List<BasketQuantityItem>();
        foreach (var entry in hashEntries)
        {
            if (!Guid.TryParse(entry.Name.ToString(), out var productId))
            {
                continue;
            }

            basketItems.Add(new BasketQuantityItem
            {
                ProductId = productId,
                Quantity = (int)entry.Value
            });
        }

        return basketItems;
    }

    public async Task RemoveFromBasket(string basketKey, Guid productId)
    {
        await _redis.HashDeleteAsync(basketKey, productId.ToString());
    }

    public async Task UpdateQuantity(string basketKey, Guid productId, int quantity)
    {
        if (quantity <= 0)
        {
            // Remove product if quantity is 0 or negative
            await RemoveFromBasket(basketKey, productId);
        }
        else
        {
            // Set new quantity (overwrite)
            await _redis.HashSetAsync(basketKey, productId.ToString(), quantity);

            // Reset TTL
            await _redis.KeyExpireAsync(basketKey, TimeSpan.FromDays(7));
        }
    }

    public async Task ClearBasket(string basketKey)
    {
        await _redis.KeyDeleteAsync(basketKey);
    }
}