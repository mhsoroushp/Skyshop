using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace Infrastructure.Data;

public class CartRepository : ICartRepository
{
    private readonly IDatabase _redis;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartRepository(IConnectionMultiplexer redis, IHttpContextAccessor httpContextAccessor)
    {
        _redis = redis.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task AddToBasket(Guid productId, int quantity)
    {
        string basketKey = GetBasketKey();
        
        // Increment quantity (auto-creates key if doesn't exist)
        await _redis.HashIncrementAsync(basketKey, productId.ToString(), quantity);
        
        // Set 7-day TTL (resets on each update)
        await _redis.KeyExpireAsync(basketKey, TimeSpan.FromDays(7));
    }

    public async Task<BasketItem[]> GetBasket()
    {
        string basketKey = GetBasketKey();
        
        // Get all products and quantities
        var hashEntries = await _redis.HashGetAllAsync(basketKey);
        
        if (hashEntries.Length == 0)
            return Array.Empty<BasketItem>();
        
        // Get product details (name, price) for each product
        var basketItems = new List<BasketItem>();
        foreach (var entry in hashEntries)
        {
            // var product = await _productService.GetProductByIdAsync(int.Parse(entry.Name));

            if (!Guid.TryParse(entry.Name.ToString(), out var productId))
            {
                continue;
            }

            // TODO :
            // Call to product service to get product details (name, price) for each productId
            basketItems.Add(new BasketItem
            {
                ProductId = productId,
                Quantity = (int)entry.Value,
                // Price = product.Price,
                // ProductName = product.Name
            });
        }
        
        return basketItems.ToArray();
    }

    public async Task RemoveFromBasket(Guid productId)
    {
        string basketKey = GetBasketKey();
        await _redis.HashDeleteAsync(basketKey, productId.ToString());
    }

    public async Task UpdateQuantity(Guid productId, int quantity)
    {
        string basketKey = GetBasketKey();
        
        if (quantity <= 0)
        {
            // Remove product if quantity is 0 or negative
            await RemoveFromBasket(productId);
        }
        else
        {
            // Set new quantity (overwrite)
            await _redis.HashSetAsync(basketKey, productId.ToString(), quantity);
            
            // Reset TTL
            await _redis.KeyExpireAsync(basketKey, TimeSpan.FromDays(7));
        }
    }

    public async Task ClearBasket()
    {
        string basketKey = GetBasketKey();
        await _redis.KeyDeleteAsync(basketKey);
    }

    public async Task<int> GetBasketItemCount()
    {
        string basketKey = GetBasketKey();
        var hashEntries = await _redis.HashGetAllAsync(basketKey);
        return hashEntries.Length;
    }

    public async Task<decimal> GetBasketTotalPrice()
    {
        var basketItems = await GetBasket();
        return basketItems.Sum(item => item.Price * item.Quantity);
    }

    private string GetBasketKey()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context available.");

        if (httpContext.Items.TryGetValue("BasketKey", out var basketKey) && basketKey is string basketKeyValue)
        {
            return basketKeyValue;
        }

        throw new InvalidOperationException("BasketKey was not set by the session middleware.");
    }
}