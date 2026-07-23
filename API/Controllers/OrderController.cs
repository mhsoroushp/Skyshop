using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using API.DTOs;
using Core.Models;
using System.Security.Claims;
using API.Extensions;

namespace API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderController(IOrderService orderService, IHttpContextAccessor httpContextAccessor)
    {
        _orderService = orderService;
        _httpContextAccessor = httpContextAccessor;
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound(new { Message = "Order not found" });

        if (!CanAccessOrder(order))
            return Forbid();

        return Ok(OrderMappingExtensions.ToOrderDto(order));
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            Console.WriteLine(request.CustomerEmail);
            Console.WriteLine(request.CustomerName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP context available.");

            string sessionId = string.Empty;
            if (httpContext.Items.TryGetValue("BasketKey", out var basketKey) && basketKey is string basketKeyValue)
            {
                sessionId = basketKeyValue;
            }

            Console.WriteLine($"Session ID: {sessionId}");

            var order = await _orderService.CreateOrderFromBasketAsync(
                sessionId,
                request.CustomerEmail,
                request.CustomerName);

            return Ok(OrderMappingExtensions.ToOrderDto(order));
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // GET: api/orders/session/{sessionId}
    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetOrderBySessionId(string sessionId)
    {
        var order = await _orderService.GetOrderBySessionIdAsync(sessionId);
        if (order == null)
            return NotFound(new { Message = "Order not found" });

        if (!CanAccessOrder(order))
            return Forbid();

        return Ok(OrderMappingExtensions.ToOrderDto(order));
    }

    private bool CanAccessOrder(Order order)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
            return string.Equals(order.OwnerUserId, userId, StringComparison.Ordinal);

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue("BasketKey", out var basketKey) == true && basketKey is string currentBasketKey)
            return string.Equals(order.SessionId, currentBasketKey, StringComparison.Ordinal);

        return false;
    }
}
