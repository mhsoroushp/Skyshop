using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public interface IStripeService
{
    Task<string> CreatePaymentIntentAsync(Guid orderId, decimal amount, string? email = null);
    Task<string?> GetPaymentIntentClientSecretAsync(string paymentIntentId);
    Task<Payment?> ConfirmPaymentAsync(string paymentIntentId, Guid paymentId);
}

public class StripeService : IStripeService
{
    private readonly IPaymentService _paymentService;
    private readonly string _stripeSecretKey;

    public StripeService(IPaymentService paymentService, IConfiguration configuration)
    {
        _paymentService = paymentService;
        _stripeSecretKey = configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe SecretKey not configured");
    }

    public async Task<string> CreatePaymentIntentAsync(Guid orderId, decimal amount, string? email = null)
    {
        try
        {
            Stripe.StripeConfiguration.ApiKey = _stripeSecretKey;

            var paymentIntentService = new Stripe.PaymentIntentService();
            var createOptions = new Stripe.PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", orderId.ToString() }
                }
            };

            if (!string.IsNullOrEmpty(email))
            {
                createOptions.ReceiptEmail = email;
            }

            var requestOptions = new Stripe.RequestOptions
            {
                IdempotencyKey = $"payment-intent-order-{orderId}"
            };

            var paymentIntent = await paymentIntentService.CreateAsync(createOptions, requestOptions);

            // Update payment with Stripe intent ID
            var existingPayment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (existingPayment != null)
            {
                existingPayment.StripePaymentIntentId = paymentIntent.Id;
                await _paymentService.UpdatePaymentStatusAsync(existingPayment.Id, PaymentStatus.Processing);
            }

            return paymentIntent.ClientSecret;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Stripe payment intent: {ex.Message}", ex);
        }
    }

    public async Task<string?> GetPaymentIntentClientSecretAsync(string paymentIntentId)
    {
        try
        {
            Stripe.StripeConfiguration.ApiKey = _stripeSecretKey;

            var paymentIntentService = new Stripe.PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

            return paymentIntent?.ClientSecret;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve Stripe payment intent: {ex.Message}", ex);
        }
    }

    public async Task<Payment?> ConfirmPaymentAsync(string paymentIntentId, Guid paymentId)
    {
        try
        {
            Stripe.StripeConfiguration.ApiKey = _stripeSecretKey;

            var paymentIntentService = new Stripe.PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

            if (paymentIntent == null)
                return null;

            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null)
                return null;

            if (paymentIntent.Status == "succeeded")
            {
                await _paymentService.UpdatePaymentStatusAsync(paymentId, PaymentStatus.Succeeded, paymentIntent.Id);
            }
            else if (paymentIntent.Status == "processing")
            {
                await _paymentService.UpdatePaymentStatusAsync(paymentId, PaymentStatus.Processing);
            }
            else if (paymentIntent.Status == "requires_payment_method" || paymentIntent.Status == "requires_action")
            {
                // Still needs action
                await _paymentService.UpdatePaymentStatusAsync(paymentId, PaymentStatus.Pending);
            }

            return await _paymentService.GetPaymentByIdAsync(paymentId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to confirm Stripe payment: {ex.Message}", ex);
        }
    }
}
