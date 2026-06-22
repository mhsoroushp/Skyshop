using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/basket")]
public class BasketController : ControllerBase
{
    private readonly ICartService _cartService;

    public BasketController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // POST: api/basket/add
    [HttpPost("add")]
    public async Task<IActionResult> AddToBasket(AddToBasketRequest request)
    {
        await _cartService.AddToBasket(request.ProductId, request.Quantity);
        return Ok(new { Message = "Product added to basket" });
    }

    // GET: api/basket
    [HttpGet]
    public async Task<IActionResult> GetBasket()
    {
        var basketItems = await _cartService.GetBasket();
        return Ok(basketItems);
    }

    // DELETE: api/basket/product/100
    [HttpDelete("product/{productId}")]
    public async Task<IActionResult> RemoveFromBasket(Guid productId)
    {
        await _cartService.RemoveFromBasket(productId);
        return Ok(new { Message = "Product removed from basket" });
    }

    // PUT: api/basket/product/100/quantity/5
    [HttpPut("product/{productId}/quantity")]
    public async Task<IActionResult> UpdateQuantity(Guid productId, UpdateQuantityRequest request)
    {
        await _cartService.UpdateQuantity(productId, request.Quantity);
        return Ok(new { Message = "Quantity updated" });
    }

    // DELETE: api/basket/clear
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearBasket()
    {
        await _cartService.ClearBasket();
        return Ok(new { Message = "Basket cleared" });
    }

    // GET: api/basket/count
    [HttpGet("count")]
    public async Task<IActionResult> GetBasketCount()
    {
        var count = await _cartService.GetBasketItemCount();
        return Ok(new { Count = count });
    }

    // GET: api/basket/total
    [HttpGet("total")]
    public async Task<IActionResult> GetBasketTotal()
    {
        var total = await _cartService.GetBasketTotalPrice();
        return Ok(new { Total = total });
    }
}

// Request Models
public class AddToBasketRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}