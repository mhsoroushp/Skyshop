using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToBasket(AddToBasketRequest request)
    {
        await _basketService.AddToBasket(request);
        return Ok(new { Message = "Product added to basket" });
    }

    [HttpGet]
    public async Task<ActionResult<BasketItem[]>> GetBasket()
    {
        var basketItems = await _basketService.GetBasket();
        return Ok(basketItems);
    }

    [HttpDelete("product/{productId}")]
    public async Task<IActionResult> RemoveFromBasket(Guid productId)
    {
        await _basketService.RemoveFromBasket(productId);
        return Ok(new { Message = "Product removed from basket" });
    }

    [HttpPut("product/{productId}/quantity")]
    public async Task<IActionResult> UpdateQuantity(Guid productId, UpdateQuantityRequest request)
    {
        await _basketService.UpdateQuantity(productId, request.Quantity);
        return Ok(new { Message = "Quantity updated" });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearBasket()
    {
        await _basketService.ClearBasket();
        return Ok(new { Message = "Basket cleared" });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetBasketCount()
    {
        var count = await _basketService.GetBasketItemCount();
        return Ok(new { Count = count });
    }

    [HttpGet("total")]
    public async Task<ActionResult> GetBasketTotalPrice()
    {
        var total = await _basketService.GetBasketTotalPrice();
        return Ok(new { Total = total });
    }
}
