using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using API.DTOs;
using API.Hubs;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Stripe;

namespace API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly ICartRepository _cartRepository;
    private readonly IStripeService _stripeService;
    private readonly IStripeWebhookEventService _stripeWebhookEventService;
    private readonly IHubContext<PaymentStatusHub> _hubContext;
    private readonly IConfiguration _configuration;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        ICartRepository cartRepository,
        IStripeService stripeService,
        IStripeWebhookEventService stripeWebhookEventService,
        IHubContext<PaymentStatusHub> hubContext,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _cartRepository = cartRepository;
        _stripeService = stripeService;
        _stripeWebhookEventService = stripeWebhookEventService;
        _hubContext = hubContext;
        _configuration = configuration;
    }

    // GET: api/payments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound(new { Message = "Payment not found" });

        var order = await _orderService.GetOrderByIdAsync(payment.OrderId);
        if (order == null)
            return NotFound(new { Message = "Order not found for this payment" });

        if (!CanAccessOrder(order))
            return Forbid();

        return Ok(MapToDto(payment));
    }

    // GET: api/payments/order/{orderId}
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        if (payment == null)
            return NotFound(new { Message = "Payment not found for this order" });

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound(new { Message = "Order not found" });

        if (!CanAccessOrder(order))
            return Forbid();

        return Ok(MapToDto(payment));
    }

    // POST: api/payments/create-intent
    [HttpPost("create-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(request.OrderId);
            if (order == null)
                return NotFound(new { Message = "Order not found" });

            // Idempotent payment creation: reuse existing payment for this order if it already exists.
            var payment = await _paymentService.CreatePaymentAsync(order.Id, order.TotalAmount);

            if (payment.Status == Core.Models.PaymentStatus.Succeeded)
            {
                return BadRequest(new { Message = "Payment already completed for this order" });
            }

            if (!string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
            {
                var existingClientSecret = await _stripeService.GetPaymentIntentClientSecretAsync(payment.StripePaymentIntentId);

                if (!string.IsNullOrWhiteSpace(existingClientSecret))
                {
                    return Ok(new CreatePaymentIntentResponse
                    {
                        ClientSecret = existingClientSecret,
                        PaymentIntentId = payment.StripePaymentIntentId
                    });
                }
            }

            // Create Stripe payment intent
            var clientSecret = await _stripeService.CreatePaymentIntentAsync(
                order.Id,
                order.TotalAmount,
                order.CustomerEmail);

            var paymentWithIntent = await _paymentService.GetPaymentByOrderIdAsync(order.Id);

            return Ok(new CreatePaymentIntentResponse
            {
                ClientSecret = clientSecret,
                PaymentIntentId = paymentWithIntent?.StripePaymentIntentId ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // POST: api/payments/confirm
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByOrderIdAsync(request.OrderId);
            if (payment == null)
                return NotFound(new { Message = "Payment not found" });

            var paymentIntentId = string.IsNullOrWhiteSpace(request.PaymentMethodId)
                ? payment.StripePaymentIntentId
                : request.PaymentMethodId;

            if (string.IsNullOrWhiteSpace(paymentIntentId))
                return BadRequest(new { Message = "Payment intent id is missing" });

            var confirmedPayment = await _stripeService.ConfirmPaymentAsync(
                paymentIntentId,
                payment.Id);

            if (confirmedPayment == null)
                return NotFound(new { Message = "Payment confirmation failed" });

            // Update order status if payment succeeded
            if (confirmedPayment.Status == Core.Models.PaymentStatus.Succeeded)
            {
                await _orderService.UpdateOrderStatusAsync(
                    request.OrderId,
                    Core.Models.OrderStatus.Confirmed);
            }

            return Ok(new
            {
                Message = "Payment processed successfully",
                Payment = MapToDto(confirmedPayment)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // POST: api/payments/webhook
    [HttpPost("webhook")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        string? stripeEventId = null;

        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = VerifyAndParseWebhookEvent(json);

            if (stripeEvent == null)
                return BadRequest(new { Message = "Invalid webhook signature" });

            stripeEventId = stripeEvent.Id;
            if (string.IsNullOrWhiteSpace(stripeEventId))
                return BadRequest(new { Message = "Webhook event id is missing" });

            var shouldProcess = await _stripeWebhookEventService.TryStartProcessingAsync(
                stripeEventId,
                stripeEvent.Type,
                json);

            if (!shouldProcess)
            {
                Console.WriteLine($"Duplicate webhook event ignored: {stripeEventId}");
                return Ok(new { Received = true, Duplicate = true });
            }

            // Handle different event types
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded(stripeEvent);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed(stripeEvent);
                    break;

                case "payment_intent.canceled":
                    await HandlePaymentIntentCanceled(stripeEvent);
                    break;

                case "charge.failed":
                    await HandleChargeFailed(stripeEvent);
                    break;

                default:
                    Console.WriteLine($"Unhandled webhook event type: {stripeEvent.Type}");
                    break;
            }

            await _stripeWebhookEventService.MarkProcessedAsync(stripeEventId);

            return Ok(new { Received = true });
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(stripeEventId))
            {
                await _stripeWebhookEventService.MarkFailedAsync(stripeEventId, ex.Message);
            }

            Console.WriteLine($"Webhook error: {ex.Message}");
            return BadRequest(new { Message = ex.Message });
        }
    }

    private Stripe.Event? VerifyAndParseWebhookEvent(string json)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(webhookSecret) || webhookSecret.Contains("YOUR_WEBHOOK_SECRET"))
            {
                Console.WriteLine("Warning: Webhook secret not configured properly");
                // For development without real webhook secret, parse without verification
                return EventUtility.ParseEvent(json);
            }

            var stripeSignature = Request.Headers["Stripe-Signature"];
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

            return stripeEvent;
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"Stripe webhook verification failed: {ex.Message}");
            return null;
        }
    }

    private async Task HandlePaymentIntentSucceeded(Stripe.Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
                return;

            Console.WriteLine($"Processing payment_intent.succeeded for {paymentIntent.Id}");

            // Find payment by Stripe PaymentIntent ID
            var payment = await _paymentService.GetPaymentByStripeIntentIdAsync(paymentIntent.Id);
            if (payment == null)
            {
                Console.WriteLine($"Payment not found for PaymentIntent {paymentIntent.Id}");
                return;
            }

            // Idempotency: do not rewrite status if already succeeded.
            // Basket clear remains safe to retry.
            if (payment.Status != Core.Models.PaymentStatus.Succeeded)
            {
                await _paymentService.UpdatePaymentStatusAsync(
                    payment.Id,
                    Core.Models.PaymentStatus.Succeeded,
                    paymentIntent.Id);
            }

            // Update order status to Confirmed
            await _orderService.UpdateOrderStatusAsync(
                payment.OrderId,
                Core.Models.OrderStatus.Confirmed);

            // Clear basket only after webhook-confirmed success.
            var order = await _orderService.GetOrderByIdAsync(payment.OrderId);
            if (!string.IsNullOrWhiteSpace(order?.SessionId))
            {
                await _cartRepository.ClearBasket(order.SessionId);
            }

            await PublishPaymentStatusUpdate(
                payment.OrderId,
                Core.Models.PaymentStatus.Succeeded.ToString(),
                Core.Models.OrderStatus.Confirmed.ToString(),
                "Payment succeeded");

            Console.WriteLine($"Payment {payment.Id} updated to Succeeded, Order {payment.OrderId} updated to Confirmed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling payment_intent.succeeded: {ex.Message}");
            throw;
        }
    }

    private async Task HandlePaymentIntentFailed(Stripe.Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
                return;

            Console.WriteLine($"Processing payment_intent.payment_failed for {paymentIntent.Id}");

            // Find payment by Stripe PaymentIntent ID
            var payment = await _paymentService.GetPaymentByStripeIntentIdAsync(paymentIntent.Id);
            if (payment == null)
            {
                Console.WriteLine($"Payment not found for PaymentIntent {paymentIntent.Id}");
                return;
            }

            // Idempotency: only update if status is not already Failed
            if (payment.Status == Core.Models.PaymentStatus.Failed)
            {
                Console.WriteLine($"Payment {payment.Id} already marked as Failed, skipping");
                return;
            }

            // Update payment status with error message
            var errorMessage = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
            await _paymentService.UpdatePaymentErrorAsync(payment.Id, errorMessage);

            // Update order status to Cancelled
            await _orderService.UpdateOrderStatusAsync(
                payment.OrderId,
                Core.Models.OrderStatus.Cancelled);

            await PublishPaymentStatusUpdate(
                payment.OrderId,
                Core.Models.PaymentStatus.Failed.ToString(),
                Core.Models.OrderStatus.Cancelled.ToString(),
                errorMessage);

            Console.WriteLine($"Payment {payment.Id} updated to Failed, Order {payment.OrderId} updated to Cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling payment_intent.payment_failed: {ex.Message}");
            throw;
        }
    }

    private async Task HandlePaymentIntentCanceled(Stripe.Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
                return;

            Console.WriteLine($"Processing payment_intent.canceled for {paymentIntent.Id}");

            // Find payment by Stripe PaymentIntent ID
            var payment = await _paymentService.GetPaymentByStripeIntentIdAsync(paymentIntent.Id);
            if (payment == null)
            {
                Console.WriteLine($"Payment not found for PaymentIntent {paymentIntent.Id}");
                return;
            }

            // Idempotency: only update if status is not already Cancelled
            if (payment.Status == Core.Models.PaymentStatus.Cancelled)
            {
                Console.WriteLine($"Payment {payment.Id} already marked as Cancelled, skipping");
                return;
            }

            // Update payment status
            await _paymentService.UpdatePaymentStatusAsync(
                payment.Id,
                Core.Models.PaymentStatus.Cancelled,
                paymentIntent.Id);

            // Update order status to Cancelled
            await _orderService.UpdateOrderStatusAsync(
                payment.OrderId,
                Core.Models.OrderStatus.Cancelled);

            await PublishPaymentStatusUpdate(
                payment.OrderId,
                Core.Models.PaymentStatus.Cancelled.ToString(),
                Core.Models.OrderStatus.Cancelled.ToString(),
                "Payment canceled");

            Console.WriteLine($"Payment {payment.Id} updated to Cancelled, Order {payment.OrderId} updated to Cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling payment_intent.canceled: {ex.Message}");
            throw;
        }
    }

    private async Task HandleChargeFailed(Stripe.Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null || charge.PaymentIntentId == null)
                return;

            Console.WriteLine($"Processing charge.failed for {charge.Id}");

            // Find payment by Stripe PaymentIntent ID
            var payment = await _paymentService.GetPaymentByStripeIntentIdAsync(charge.PaymentIntentId);
            if (payment == null)
            {
                Console.WriteLine($"Payment not found for PaymentIntent {charge.PaymentIntentId}");
                return;
            }

            // Update payment status with error message
            var errorMessage = charge.FailureMessage ?? "Charge failed";
            await _paymentService.UpdatePaymentErrorAsync(payment.Id, errorMessage);

            await PublishPaymentStatusUpdate(
                payment.OrderId,
                Core.Models.PaymentStatus.Failed.ToString(),
                Core.Models.OrderStatus.Cancelled.ToString(),
                errorMessage);

            Console.WriteLine($"Payment {payment.Id} error updated: {errorMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling charge.failed: {ex.Message}");
            throw;
        }
    }

    private static PaymentDto MapToDto(Core.Models.Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            PaymentMethod = payment.PaymentMethod.ToString(),
            TransactionId = payment.TransactionId,
            StripePaymentIntentId = payment.StripePaymentIntentId,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt
        };
    }

    private async Task PublishPaymentStatusUpdate(Guid orderId, string paymentStatus, string orderStatus, string message)
    {
        await _hubContext.Clients
            .Group(PaymentStatusHub.OrderGroup(orderId))
            .SendAsync("PaymentStatusUpdated", new
            {
                orderId,
                paymentStatus,
                orderStatus,
                message,
                updatedAt = DateTime.UtcNow
            });
    }

    private bool CanAccessOrder(Core.Models.Order order)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
            return string.Equals(order.OwnerUserId, userId, StringComparison.Ordinal);

        if (HttpContext.Items.TryGetValue("BasketKey", out var basketKey) && basketKey is string currentBasketKey)
            return string.Equals(order.SessionId, currentBasketKey, StringComparison.Ordinal);

        return false;
    }
}
