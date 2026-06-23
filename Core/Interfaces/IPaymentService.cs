using Core.Models;

namespace Core.Interfaces;

public interface IPaymentService
{
    Task<Payment> CreatePaymentAsync(Guid orderId, decimal amount);
    Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
    Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId);
    Task<Payment?> GetPaymentByStripeIntentIdAsync(string stripePaymentIntentId);
    Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string? transactionId = null);
    Task UpdatePaymentErrorAsync(Guid paymentId, string errorMessage);
}
