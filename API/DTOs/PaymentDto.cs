namespace API.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? Email { get; set; }
}

public class CreatePaymentIntentRequest
{
    public Guid OrderId { get; set; }
}

public class CreatePaymentIntentResponse
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
}

public class StripeWebhookRequest
{
    public string Type { get; set; } = string.Empty;
    public StripePaymentIntentData Data { get; set; } = new();
}

public class StripePaymentIntentData
{
    public StripePaymentIntentObject Object { get; set; } = new();
}

public class StripePaymentIntentObject
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
