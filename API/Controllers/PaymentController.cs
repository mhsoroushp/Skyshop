using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using API.DTOs;
using Infrastructure.Services;

namespace API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IStripeService _stripeService;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        IStripeService stripeService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _stripeService = stripeService;
    }

    // GET: api/payments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound(new { Message = "Payment not found" });

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

            // Create payment record
            var payment = await _paymentService.CreatePaymentAsync(order.Id, order.TotalAmount);

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
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            
            // In production, verify webhook signature
            // For now, just parse and handle the event

            var payment = new Core.Models.Payment();
            // Process webhook (implementation depends on Stripe webhook type)

            return Ok(new { Received = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
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
}
