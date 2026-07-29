using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Data;

public class BasketService : IBasketService
{
	private readonly IBasketRepository _basketRepo;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IBookRepository _bookRepo;
	private readonly IBlobStorageService _blobService;

	public BasketService(
		IBasketRepository basketRepository, 
		IHttpContextAccessor httpContextAccessor, 
		IBookRepository bookRepository,
		IBlobStorageService blobStorageService
		)
	{
		_basketRepo = basketRepository;
		_httpContextAccessor = httpContextAccessor;
		_bookRepo = bookRepository;
		_blobService = blobStorageService;
	}

	public async Task AddToBasket(AddToBasketRequest request)
	{
		string basketKey = GetBasketKey();
		await _basketRepo.AddToBasket(basketKey, request.ProductId, request.Quantity);
	}

	public async Task<List<BasketItem>> GetBasket()
	{
		string basketKey = GetBasketKey();

		var basketEntries = await _basketRepo.GetBasket(basketKey);

		if (basketEntries.Count == 0)
		{
			return [];
		}
				

		var basketItems = new List<BasketItem>();
		foreach (var entry in basketEntries)
		{
			var book = await _bookRepo.GetByIdAsync(entry.ProductId);
			if (book is null)
			{
				Console.WriteLine($"Book not found for ProductId: {entry.ProductId}. Removing from basket.");
				await _basketRepo.RemoveFromBasket(basketKey, entry.ProductId);
				continue;
			}

			try
			{
				// get product image from azure blob storage
				var imageBytes = await _blobService.DownloadImageAsBytesAsync(book.CoverImageUrl ?? string.Empty);
				basketItems.Add(new BasketItem
				{
					ProductId = entry.ProductId,
					Quantity = entry.Quantity,
					Price = book.Price,
					ProductName = book.Title,
					ProductImageBase64 = Convert.ToBase64String(imageBytes)
				});
				
			}catch (Exception ex)
			{
				Console.WriteLine($"Error fetching book details for ProductId: {entry.ProductId}. Exception: {ex.Message}");
				await _basketRepo.RemoveFromBasket(basketKey, entry.ProductId);
				continue;
			}
		}
		return basketItems;
	}

	public async Task RemoveFromBasket(Guid productId)
	{
		string basketKey = GetBasketKey();
		await _basketRepo.RemoveFromBasket(basketKey, productId);
	}

	public async Task UpdateQuantity(Guid productId, int quantity)
	{
		string basketKey = GetBasketKey();
		await _basketRepo.UpdateQuantity(basketKey, productId, quantity);
	}

	public async Task ClearBasket()
	{
		string basketKey = GetBasketKey();
		await _basketRepo.ClearBasket(basketKey);
	}

	public async Task<int> GetBasketItemCount()
	{
		var basketItems = await GetBasket();
		return basketItems.Sum(item => item.Quantity);
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
