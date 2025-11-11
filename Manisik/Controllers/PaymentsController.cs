using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Manisik.DTOs;
using Manisik.Services.Payment;
using Manisik.Interfaces;
using Stripe;

namespace Manisik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly StripePaymentService _stripe;
        private readonly PayPalPaymentService _paypal;
        private readonly IUmrahBookingRepository _bookingRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            StripePaymentService stripe,
            PayPalPaymentService paypal,
            IUmrahBookingRepository bookingRepo,
            IConfiguration config,
            ILogger<PaymentsController> logger)
        {
            _stripe = stripe;
            _paypal = paypal;
            _bookingRepo = bookingRepo;
            _config = config;
            _logger = logger;
        }

        // ✅ Create Stripe PaymentIntent
        [HttpPost("stripe/create-intent")]
        public async Task<IActionResult> CreateStripeIntent([FromBody] StripeCreateIntentDto dto)
        {
            if (dto.Amount <= 0)
                return BadRequest(new { message = "❌ Amount must be greater than 0" });

            var resp = await _stripe.CreatePaymentIntentAsync(dto);

            return Ok(new
            {
                message = "✅ Stripe payment intent created successfully",
                clientSecret = resp.ClientSecret,
                amount = dto.Amount
            });
        }

        // ✅ Stripe Webhook
        [HttpPost("stripe/webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var sig = Request.Headers["Stripe-Signature"].FirstOrDefault();
            try
            {
                var ev = _stripe.ConstructEvent(json, sig);
                _logger.LogInformation("Stripe event received: {Type}", ev.Type);

                if (ev.Type == "payment_intent.succeeded")
                {
                    var pi = ev.Data.Object as PaymentIntent;
                    _logger.LogInformation("PaymentIntent succeeded: {Id}", pi?.Id);

                    if (pi != null)
                    {
                        await _bookingRepo.TryMarkBookingPaidByProviderEventAsync("Stripe", pi.Id, ev.Id, null);
                    }
                }

                return Ok(new { message = "✅ Stripe webhook received successfully" });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook validation failed");
                return BadRequest(new { message = "❌ Stripe webhook validation failed" });
            }
        }

        // ✅ Create PayPal order
        [HttpPost("paypal/create-order")]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] PayPalCreateOrderDto dto)
        {
            if (dto.Amount <= 0)
                return BadRequest(new { message = "❌ Amount must be greater than 0" });

            var resp = await _paypal.CreateOrderAsync(dto);
            if (resp == null)
                return BadRequest(new { message = "❌ Failed to create PayPal order" });

            return Ok(new
            {
                message = "✅ PayPal order created successfully",
                data = resp
            });
        }

        // ✅ Capture PayPal order
        [HttpPost("paypal/capture/{orderId}")]
        public async Task<IActionResult> CapturePayPalOrder(string orderId)
        {
            var json = await _paypal.CaptureOrderAsync(orderId);
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("Failed to capture PayPal order {OrderId}", orderId);
                return BadRequest(new { message = "❌ Failed to capture PayPal order" });
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var status = root.GetProperty("status").GetString();

                string eventId = string.Empty;
                if (root.TryGetProperty("purchase_units", out var pus) && pus.ValueKind == JsonValueKind.Array)
                {
                    foreach (var pu in pus.EnumerateArray())
                    {
                        if (pu.TryGetProperty("payments", out var payments) && payments.ValueKind == JsonValueKind.Object)
                        {
                            if (payments.TryGetProperty("captures", out var captures) && captures.ValueKind == JsonValueKind.Array)
                            {
                                var first = captures.EnumerateArray().FirstOrDefault();
                                if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("id", out var cid))
                                {
                                    eventId = cid.GetString() ?? string.Empty;
                                }
                            }
                        }
                    }
                }

                await _bookingRepo.TryMarkBookingPaidByProviderEventAsync("PayPal", orderId, eventId, DateTime.UtcNow);

                return Ok(new
                {
                    message = "✅ PayPal payment captured successfully",
                    status = status,
                    orderId = orderId
                });
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Failed to parse PayPal capture JSON for order {OrderId}", orderId);
                return BadRequest(new { message = "❌ Error parsing PayPal response" });
            }
        }
    }
}
