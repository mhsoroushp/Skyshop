using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/basket")]
public class BasketController : ControllerBase
{
    private readonly ICartRepository _cartRepository;

    public BasketController(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    // POST: api/basket/add
    [HttpPost("add")]
    public async Task<IActionResult> AddToBasket(AddToBasketRequest request)
    {
        await _cartRepository.AddToBasket(request.ProductId, request.Quantity);
        return Ok(new { Message = "Product added to basket" });
    }

    // GET: api/basket
    [HttpGet]
    public async Task<IActionResult> GetBasket()
    {
        var basketItems = await _cartRepository.GetBasket();
        return Ok(basketItems);
    }

    // DELETE: api/basket/product/100
    [HttpDelete("product/{productId}")]
    public async Task<IActionResult> RemoveFromBasket(int productId)
    {
        await _cartRepository.RemoveFromBasket(productId);
        return Ok(new { Message = "Product removed from basket" });
    }

    // PUT: api/basket/product/100/quantity/5
    [HttpPut("product/{productId}/quantity")]
    public async Task<IActionResult> UpdateQuantity(int productId, UpdateQuantityRequest request)
    {
        await _cartRepository.UpdateQuantity(productId, request.Quantity);
        return Ok(new { Message = "Quantity updated" });
    }

    // DELETE: api/basket/clear
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearBasket()
    {
        await _cartRepository.ClearBasket();
        return Ok(new { Message = "Basket cleared" });
    }

    // GET: api/basket/count
    [HttpGet("count")]
    public async Task<IActionResult> GetBasketCount()
    {
        var count = await _cartRepository.GetBasketItemCount();
        return Ok(new { Count = count });
    }

    // GET: api/basket/total
    [HttpGet("total")]
    public async Task<IActionResult> GetBasketTotal()
    {
        var total = await _cartRepository.GetBasketTotalPrice();
        return Ok(new { Total = total });
    }
}

// Request Models
public class AddToBasketRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}