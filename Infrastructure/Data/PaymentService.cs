using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Data;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Payment> CreatePaymentAsync(Guid orderId, decimal amount)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);
        return payment;
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
    {
        return await _paymentRepository.GetByIdAsync(paymentId);
    }

    public async Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId)
    {
        return await _paymentRepository.GetByOrderIdAsync(orderId);
    }

    public async Task<Payment?> GetPaymentByStripeIntentIdAsync(string stripePaymentIntentId)
    {
        return await _paymentRepository.GetByStripeIntentIdAsync(stripePaymentIntentId);
    }

    public async Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string? transactionId = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new InvalidOperationException($"Payment with id {paymentId} not found");

        payment.Status = status;
        payment.TransactionId = transactionId;
        
        if (status == PaymentStatus.Succeeded)
            payment.ProcessedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment);
    }

    public async Task UpdatePaymentErrorAsync(Guid paymentId, string errorMessage)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new InvalidOperationException($"Payment with id {paymentId} not found");

        payment.Status = PaymentStatus.Failed;
        payment.ErrorMessage = errorMessage;
        await _paymentRepository.UpdateAsync(payment);
    }
}
