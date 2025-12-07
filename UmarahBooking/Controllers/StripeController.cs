using Manisik.Enums;
using Manisik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stripe;
using Newtonsoft.Json.Linq;
using UmarahBooking.Core.Interfaces;
using System.IO;
using PaymentMethod = Manisik.Enums.PaymentMethod;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("payment")]
    public class StripeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly ILogger<StripeController> _logger;

        public StripeController(IUnitOfWork unitOfWork, IConfiguration config, ILogger<StripeController> logger)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _logger = logger;

            // set API key for server-side calls
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            if (request == null || request.BookingId <= 0)
                return BadRequest(new { message = "Invalid request" });

            // Load booking (including TotalPrice)
            var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
            if (booking == null) return NotFound(new { message = "Booking not found" });

            var totalAmount = booking.TotalPrice ?? 0m;
            if (totalAmount <= 0) return BadRequest(new { message = "Total amount cannot be zero." });

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(totalAmount * 100M),
                Currency = (request.Currency ?? "usd").ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                Metadata = new Dictionary<string, string>
                {
                    { "bookingId", booking.BookingId.ToString() },
                    { "bookingNumber", booking.BookingNumber ?? string.Empty },
                    { "userId", booking.UserId.ToString() ?? string.Empty },
                    { "tripType", booking.TripType.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var requestOptions = string.IsNullOrEmpty(request.IdempotencyKey)
                ? null
                : new RequestOptions { IdempotencyKey = request.IdempotencyKey };

            var pi = await service.CreateAsync(options, requestOptions);

            // Persist payment record (status = Pending)
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = totalAmount,
                Currency = options.Currency?.ToUpper(),
                PaymentMethod = PaymentMethod.Stripe,
                Status = PaymentStatus.Pending,
                PaymentIntentId = pi.Id,
                ClientSecret = pi.ClientSecret,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChanges();

            return Ok(new
            {
                clientSecret = pi.ClientSecret,
                amount = totalAmount,
                currency = options.Currency
            });
        }

        /// <summary>
        /// Called by frontend after successful client-side payment confirmation.
        /// This is a fallback to update booking status in case webhook is delayed or not configured.
        /// </summary>
        [HttpPost("ConfirmPayment")]
        [Authorize]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            if (string.IsNullOrEmpty(request?.PaymentIntentId))
                return BadRequest(new { message = "PaymentIntentId is required" });

            try
            {
                // Verify the payment intent with Stripe
                var service = new PaymentIntentService();
                var pi = await service.GetAsync(request.PaymentIntentId);

                if (pi == null)
                    return NotFound(new { message = "Payment not found" });

                if (pi.Status != "succeeded")
                    return BadRequest(new { message = $"Payment not successful. Status: {pi.Status}" });

                // Extract bookingId from metadata
                if (!pi.Metadata.TryGetValue("bookingId", out var bookingIdStr) || !int.TryParse(bookingIdStr, out var bookingId))
                    return BadRequest(new { message = "Could not determine booking from payment" });

                // Update booking status
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                    return NotFound(new { message = "Booking not found" });

                // Only update if currently pending (avoid overwriting if webhook already ran)
                if (booking.BookingStatus == BookingStatus.Pending)
                {
                    booking.BookingStatus = BookingStatus.Confirmed;
                    booking.PaymentStatus = PaymentStatus.Paid;
                    booking.PaymentIntentId = pi.Id;
                    booking.PaymentMethod = PaymentMethod.Stripe;
                    booking.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    await _unitOfWork.SaveChanges();

                    _logger.LogInformation("Booking {BookingId} confirmed via frontend callback", bookingId);
                }

                return Ok(new { 
                    success = true, 
                    message = "Payment confirmed successfully",
                    bookingId = bookingId,
                    status = booking.BookingStatus.ToString()
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error verifying payment intent {PaymentIntentId}", request.PaymentIntentId);
                return BadRequest(new { message = "Could not verify payment with Stripe" });
            }
        }

        // Stripe webhook to receive events (payment succeeded, failed, etc.)
        [HttpPost("Webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var sigHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();
            var webhookSecret = _config["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogWarning("Stripe webhook secret not configured.");
                return BadRequest();
            }

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, sigHeader, webhookSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Stripe webhook signature");
                return BadRequest();
            }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var pi = ((JObject)stripeEvent.Data.Object).ToObject<PaymentIntent>();
                if (pi == null)
                {
                    _logger.LogWarning("PaymentIntent payload missing");
                    return BadRequest();
                }

                // Find payment record by PaymentIntentId
                var payment = await _unitOfWork.Payments.FindBySearch(p => p.PaymentIntentId == pi.Id);
                if (payment == null)
                {
                    _logger.LogWarning("Payment record not found for intent {IntentId}", pi.Id);
                    return NotFound();
                }

                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                await _unitOfWork.Payments.UpdateAsync(payment);

                // Mark booking as confirmed (business rule)
                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.BookingStatus = BookingStatus.Confirmed;
                    booking.PaymentStatus = PaymentStatus.Paid; // or map to your enum
                    booking.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                }

                await _unitOfWork.SaveChanges();
                return Ok();
            }

            if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var pi = ((JObject)stripeEvent.Data.Object).ToObject<PaymentIntent>();
                var payment = await _unitOfWork.Payments.FindBySearch(p => p.PaymentIntentId == pi.Id);
                if (payment != null)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChanges();
                }

                return Ok();
            }

            // handle other events as needed
            return Ok();
        }
    }

    public class CreatePaymentRequest
    {
        public int BookingId { get; set; }
        public string Currency { get; set; } = "usd";
        public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}