using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using API.DTOs;
using Core.Models;

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
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound(new { Message = "Order not found" });

        return Ok(MapToDto(order));
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

            return Ok(new
            {
                Message = "Order created successfully",
                Order = MapToDto(order)
            });
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

        return Ok(MapToDto(order));
    }

    // TODO: move this the extension method
    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            SessionId = order.SessionId,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            CustomerEmail = order.CustomerEmail,
            CustomerName = order.CustomerName,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price,
                ProductName = i.ProductName
            }).ToList()
        };
    }
}
