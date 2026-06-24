using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Data;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartService _cartService;
    private readonly IBookRepository _bookRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderService(
        IOrderRepository orderRepository,
        ICartService cartService,
        IBookRepository bookRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
        _bookRepository = bookRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Order> CreateOrderFromBasketAsync(string sessionId, string? customerEmail = null, string? customerName = null)
    {
        // Get basket items
        var basketItems = await _cartService.GetBasket();

        if (basketItems.Length == 0)
            throw new InvalidOperationException("Cannot create order from empty basket");

        // Calculate total
        var totalAmount = basketItems.Sum(item => item.Price * item.Quantity);

        // Create order
        var order = new Order
        {
            SessionId = sessionId,
            OwnerUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            TotalAmount = totalAmount,
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            Status = OrderStatus.Pending
        };

        // Add order items
        foreach (var basketItem in basketItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = basketItem.ProductId,
                Quantity = basketItem.Quantity,
                Price = basketItem.Price,
                ProductName = basketItem.ProductName
            });
        }

        // Save order
        await _orderRepository.AddAsync(order);

        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _orderRepository.GetByIdAsync(orderId);
    }

    public async Task<Order?> GetOrderBySessionIdAsync(string sessionId)
    {
        return await _orderRepository.GetBySessionIdAsync(sessionId);
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order with id {orderId} not found");

        order.Status = status;
        await _orderRepository.UpdateAsync(order);
    }

    public async Task<IEnumerable<Order>> GetOrdersBySessionIdAsync(string sessionId)
    {
        return await _orderRepository.GetBySessionIdAllAsync(sessionId);
    }
}
