using Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Data;

public class CartService : ICartService
{
	private readonly ICartRepository _cartRepository;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IBookRepository _bookRepository;

	public CartService(ICartRepository cartRepository, IHttpContextAccessor httpContextAccessor, IBookRepository bookRepository)
	{
		_cartRepository = cartRepository;
		_httpContextAccessor = httpContextAccessor;
		_bookRepository = bookRepository;
	}

	public async Task AddToBasket(Guid productId, int quantity)
	{
		string basketKey = GetBasketKey();
		await _cartRepository.AddToBasket(basketKey, productId, quantity);
	}

	public async Task<BasketItem[]> GetBasket()
	{
		string basketKey = GetBasketKey();

		var basketEntries = await _cartRepository.GetBasket(basketKey);

		if (basketEntries.Length == 0)
			return Array.Empty<BasketItem>();

		var basketItems = new List<BasketItem>();
		foreach (var entry in basketEntries)
		{
			var book = await _bookRepository.GetByIdAsync(entry.ProductId);
			if (book is null)
			{
				await _cartRepository.RemoveFromBasket(basketKey, entry.ProductId);
				continue;
			}

			basketItems.Add(new BasketItem
			{
				ProductId = entry.ProductId,
				Quantity = entry.Quantity,
				Price = book.Price,
				ProductName = book.Title
			});
		}

		return basketItems.ToArray();
	}

	public async Task RemoveFromBasket(Guid productId)
	{
		string basketKey = GetBasketKey();
		await _cartRepository.RemoveFromBasket(basketKey, productId);
	}

	public async Task UpdateQuantity(Guid productId, int quantity)
	{
		string basketKey = GetBasketKey();
		await _cartRepository.UpdateQuantity(basketKey, productId, quantity);
	}

	public async Task ClearBasket()
	{
		string basketKey = GetBasketKey();
		await _cartRepository.ClearBasket(basketKey);
	}

	public async Task<int> GetBasketItemCount()
	{
		var basketItems = await GetBasket();
		return basketItems.Length;
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
