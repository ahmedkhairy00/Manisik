using Microsoft.AspNetCore.Mvc;
using Stripe;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;
using UmarahBooking.Core.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public PaymentController(IUnitOfWork unitOfWork, ILogger<PaymentController> logger, IConfiguration configuration, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("CreatePayment")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto request)
        {
            if (request == null || request.BookingId <= 0)
                return BadRequest(new { message = "Invalid payment request" });

            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
                if (booking == null)
                    return NotFound(new { message = "Booking not found" });

                // Determine amount
                decimal amount = request.Amount ?? booking.TotalPrice ?? 0m;
                if (amount <= 0)
                    return BadRequest(new { message = "Invalid amount for payment" });

                // Convert amount to smallest currency unit (cents)
                var amountInCents = Convert.ToInt64(Math.Round(amount * 100M));
                var currency = (request.Currency ?? "usd").ToLower();

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currency,
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "bookingId", booking.BookingId.ToString() }
                    }
                };

                var requestOptions = new RequestOptions();
                if (!string.IsNullOrEmpty(request.IdempotencyKey))
                {
                    requestOptions.IdempotencyKey = request.IdempotencyKey;
                }

                var service = new PaymentIntentService();
                var pi = await service.CreateAsync(options, requestOptions);

                // Create payment record
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = amount,
                    Currency = currency,
                    PaymentMethod = UmarahBooking.Core.Enums.PaymentMethod.Stripe,
                    Status = UmarahBooking.Core.Enums.PaymentStatus.Pending,
                    PaymentIntentId = pi.Id,
                    ClientSecret = pi.ClientSecret,
                    CreatedAt = DateTime.UtcNow,
                    IdempotencyKey = request.IdempotencyKey
                };

                await _unitOfWork.Payments.AddAsync(payment);

                // Mark booking payment status as pending and set ReservedUntil to give user time to complete payment
                booking.PaymentStatus = UmarahBooking.Core.Enums.PaymentStatus.Pending;
                booking.BookingStatus = UmarahBooking.Core.Enums.BookingStatus.Pending;
                booking.ReservedUntil = DateTime.UtcNow.AddMinutes(30);
                await _unitOfWork.Bookings.UpdateAsync(booking);

                await _unitOfWork.SaveChanges();

                var response = new CreatePaymentResponseDto
                {
                    ClientSecret = pi.ClientSecret,
                    PaymentIntentId = pi.Id
                };

                // Return raw object to match frontend expectations
                return Ok(response);
            }
            catch (StripeException sx)
            {
                _logger.LogError(sx, "Stripe error while creating payment");
                return StatusCode(500, new { message = "Stripe error while creating payment" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating payment");
                return StatusCode(500, new { message = "Error while creating payment" });
            }
        }

        // Stripe webhook endpoint - should be configured in Stripe dashboard to send events here
        [HttpPost("Webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            Event stripeEvent = null;

            try
            {
                if (!string.IsNullOrEmpty(webhookSecret) && !string.IsNullOrEmpty(signature))
                {
                    stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
                }
                else
                {
                    // If webhook secret not configured, try to parse directly (not recommended for production)
                    stripeEvent = EventUtility.ParseEvent(json);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse Stripe webhook event");
                return BadRequest();
            }

            try
            {
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var pi = stripeEvent.Data.Object as PaymentIntent;
                        if (pi != null)
                        {
                            var payment = await _unitOfWork.Payments
                                .GetAllAsQuerable()
                                .Where(p => p.PaymentIntentId == pi.Id)
                                .FirstOrDefaultAsync();

                            if (payment != null)
                            {
                                payment.Status = UmarahBooking.Core.Enums.PaymentStatus.Paid;
                                payment.PaidAt = DateTime.UtcNow;
                                // use LatestChargeId available on PaymentIntent in newer Stripe.NET versions
                                payment.TransactionId = pi.LatestChargeId;
                                await _unitOfWork.Payments.UpdateAsync(payment);

                                var booking = await _unitOfWork.Bookings.FindWithAsync(b => b.BookingId == payment.BookingId, new[] { "User" });
                                var bookingEntity = booking.FirstOrDefault();
                                
                                if (bookingEntity != null)
                                {
                                    bookingEntity.PaymentStatus = UmarahBooking.Core.Enums.PaymentStatus.Paid;
                                    bookingEntity.BookingStatus = UmarahBooking.Core.Enums.BookingStatus.Confirmed;
                                    bookingEntity.PaymentIntentId = pi.Id;
                                    bookingEntity.PaymentDate = DateTime.UtcNow;
                                    await _unitOfWork.Bookings.UpdateAsync(bookingEntity);
                                    
                                    // Send Success Email
                                    if (bookingEntity.User != null && !string.IsNullOrEmpty(bookingEntity.User.Email))
                                    {
                                        _ = _emailService.SendPaymentSuccessEmailAsync(
                                            bookingEntity.User.Email, 
                                            bookingEntity.BookingNumber ?? bookingEntity.BookingId.ToString(), 
                                            payment.Amount,
                                            bookingEntity.User.FullName ?? "Customer",
                                            bookingEntity.TravelStartDate ?? DateTime.UtcNow,
                                            bookingEntity.TravelEndDate ?? bookingEntity.TravelStartDate ?? DateTime.UtcNow,
                                            bookingEntity.TripType.ToString());
                                    }
                                }

                                await _unitOfWork.SaveChanges();
                            }
                        }
                        break;

                    case "payment_intent.payment_failed":
                        var piFail = stripeEvent.Data.Object as PaymentIntent;
                        if (piFail != null)
                        {
                            var payment = await _unitOfWork.Payments
                                .GetAllAsQuerable()
                                .Where(p => p.PaymentIntentId == piFail.Id)
                                .FirstOrDefaultAsync();

                            if (payment != null)
                            {
                                payment.Status = UmarahBooking.Core.Enums.PaymentStatus.Failed;
                                payment.FailureReason = piFail.LastPaymentError?.Message;
                                await _unitOfWork.Payments.UpdateAsync(payment);
                                await _unitOfWork.SaveChanges();
                            }
                        }
                        break;

                    // Add other events you care about
                    default:
                        _logger.LogInformation("Unhandled stripe event type: {Type}", stripeEvent.Type);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling webhook event");
                return StatusCode(500);
            }

            return Ok();
        }
    }
}

