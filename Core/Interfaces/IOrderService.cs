using Core.Models;

namespace Core.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderFromBasketAsync(string sessionId, string? customerEmail = null, string? customerName = null);
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<Order?> GetOrderBySessionIdAsync(string sessionId);
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
    Task<IEnumerable<Order>> GetOrdersBySessionIdAsync(string sessionId);
}
