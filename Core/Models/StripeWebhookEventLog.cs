namespace Core.Models;

public class StripeWebhookEventLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string StripeEventId { get; set; }
    public required string EventType { get; set; }
    public bool Processed { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Payload { get; set; }
    public string? ErrorMessage { get; set; }
}