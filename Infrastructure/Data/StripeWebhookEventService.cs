using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class StripeWebhookEventService : IStripeWebhookEventService
{
    private readonly BookContext _context;

    public StripeWebhookEventService(BookContext context)
    {
        _context = context;
    }

    public async Task<bool> TryStartProcessingAsync(string stripeEventId, string eventType, string payload)
    {
        var existing = await _context.StripeWebhookEventLogs
            .FirstOrDefaultAsync(e => e.StripeEventId == stripeEventId);

        if (existing != null)
            return false;

        var log = new StripeWebhookEventLog
        {
            StripeEventId = stripeEventId,
            EventType = eventType,
            Processed = false,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow
        };

        _context.StripeWebhookEventLogs.Add(log);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Protects against race conditions where the same event is inserted in parallel.
            return false;
        }
    }

    public async Task MarkProcessedAsync(string stripeEventId)
    {
        var log = await _context.StripeWebhookEventLogs
            .FirstOrDefaultAsync(e => e.StripeEventId == stripeEventId);

        if (log == null)
            return;

        log.Processed = true;
        log.ProcessedAt = DateTime.UtcNow;
        log.ErrorMessage = null;

        await _context.SaveChangesAsync();
    }

    public async Task MarkFailedAsync(string stripeEventId, string errorMessage)
    {
        var log = await _context.StripeWebhookEventLogs
            .FirstOrDefaultAsync(e => e.StripeEventId == stripeEventId);

        if (log == null)
            return;

        log.Processed = false;
        log.ErrorMessage = errorMessage;

        await _context.SaveChangesAsync();
    }
}