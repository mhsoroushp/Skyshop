using Core.Models;

namespace Core.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<Payment?> GetByStripeIntentIdAsync(string stripePaymentIntentId);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
