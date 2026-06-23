using Core.Models;

namespace Core.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<Order>> GetBySessionIdAllAsync(string sessionId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
