using Core.Enums;

namespace Core.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
    public string? TransactionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public Order? Order { get; set; }
}