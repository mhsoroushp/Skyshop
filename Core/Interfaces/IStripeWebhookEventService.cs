namespace Core.Interfaces;

public interface IStripeWebhookEventService
{
    Task<bool> TryStartProcessingAsync(string stripeEventId, string eventType, string payload);
    Task MarkProcessedAsync(string stripeEventId);
    Task MarkFailedAsync(string stripeEventId, string errorMessage);
}