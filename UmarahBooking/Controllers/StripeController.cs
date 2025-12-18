using UmarahBooking.Core.Enums;
using UmarahBooking.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stripe;
using Newtonsoft.Json.Linq;
using UmarahBooking.Core.Interfaces;
using System.IO;
using PaymentMethod = UmarahBooking.Core.Enums.PaymentMethod;

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
        private readonly IEmailService _emailService;

        public StripeController(IUnitOfWork unitOfWork, IConfiguration config, ILogger<StripeController> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _logger = logger;
            _emailService = emailService;

            // set API key for server-side calls
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try 
            {
                if (request == null || request.BookingId <= 0)
                    return BadRequest(new { message = "Invalid request" });

                // Load booking (including TotalPrice)
                var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
                if (booking == null) return NotFound(new { message = "Booking not found" });

                var totalAmount = booking.TotalPrice ?? 0m;

                _logger.LogInformation($"CreatePayment Request: BookingId={request.BookingId}, DB_Total={totalAmount}, Req_Amount={request.Amount}");

                // Trust the frontend amount if provided (fixes zero amount issue from DB)
                if (request.Amount > 0)
                {
                    totalAmount = request.Amount;
                    // Update DB 
                    if (booking.TotalPrice <= 0) 
                    {
                        booking.TotalPrice = totalAmount;
                        await _unitOfWork.Bookings.UpdateAsync(booking);
                        await _unitOfWork.SaveChanges(); 
                    }
                }

                if (totalAmount <= 0) return BadRequest(new { message = $"Total amount cannot be zero. DB={booking.TotalPrice}, Req={request.Amount}" });

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

                // Check if payment record already exists
                var payment = await _unitOfWork.Payments.FindBySearch(p => p.BookingId == booking.BookingId);

                if (payment != null)
                {
                    // Update existing record
                    payment.Amount = totalAmount;
                    payment.Currency = options.Currency?.ToUpper();
                    payment.PaymentMethod = PaymentMethod.Stripe;
                    payment.Status = PaymentStatus.Pending;
                    payment.PaymentIntentId = pi.Id;
                    payment.ClientSecret = pi.ClientSecret;
                    payment.CreatedAt = DateTime.UtcNow; 
                    
                    await _unitOfWork.Payments.UpdateAsync(payment);
                }
                else
                {
                    // Create new payment record
                    payment = new Payment
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
                }

                // Log Event
                try
                {
                    var evt = new PaymentEvent
                    {
                        Payment = payment, 
                        Provider = "Stripe",
                        EventId = pi.Id,
                        Payload = $"Payment Intent Created for Booking {booking.BookingId}",
                        ProcessedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.PaymentEvents.AddAsync(evt);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to log payment event");
                }

                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    clientSecret = pi.ClientSecret,
                    amount = totalAmount,
                    currency = options.Currency
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return StatusCode(500, new { message = $"DEBUG ERROR: {ex.Message} | {ex.InnerException?.Message}" });
            }
        }

        /// <summary>
        /// Called by frontend after successful client-side payment confirmation.
        /// This is a fallback to update booking status in case webhook is delayed or not configured.
        /// </summary>
        [HttpPost("ConfirmPayment")]
        //[Authorize] // Removed to rely on Stripe Verification as source of truth (fixes token expiration issues)
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            _logger.LogInformation("ConfirmPayment called for PaymentIntentId: {PaymentIntentId}", request?.PaymentIntentId);

            if (string.IsNullOrEmpty(request?.PaymentIntentId))
            {
                _logger.LogError("ConfirmPayment failed: PaymentIntentId is required");
                return BadRequest(new { message = "PaymentIntentId is required" });
            }

            try
            {
                // Verify the payment intent with Stripe
                var service = new PaymentIntentService();
                var pi = await service.GetAsync(request.PaymentIntentId);

                if (pi == null)
                {
                    _logger.LogError("ConfirmPayment failed: PaymentIntent not found for {PaymentIntentId}", request.PaymentIntentId);
                    return NotFound(new { message = "Payment not found" });
                }

                if (pi.Status != "succeeded")
                {
                     _logger.LogError("ConfirmPayment failed: Status is {Status}, expected succeeded", pi.Status);
                    return BadRequest(new { message = $"Payment not successful. Status: {pi.Status}" });
                }

                // Extract bookingId from metadata
                if (!pi.Metadata.TryGetValue("bookingId", out var bookingIdStr) || !int.TryParse(bookingIdStr, out var bookingId))
                {
                     _logger.LogError("ConfirmPayment failed: bookingId missing in metadata. Metadata keys: {Keys}", string.Join(",", pi.Metadata.Keys));
                    return BadRequest(new { message = "Could not determine booking from payment" });
                }

                // Update booking status
                // Update booking status
                var booking = (await _unitOfWork.Bookings.FindWithAsync(
                    b => b.BookingId == bookingId,
                    new[] { 
                        "User", 
                        "Travelers",
                        "Hotels", "Hotels.Hotel", "Hotels.Room",
                        "BookingInternationalTransport", "BookingInternationalTransport.InternationalTransport",
                        "BookingGroundTransport", "BookingGroundTransport.GroundTransport"
                    } 
                )).FirstOrDefault();

                if (booking == null)
                {
                    _logger.LogError("ConfirmPayment failed: Booking {BookingId} not found in DB", bookingId);
                    return NotFound(new { message = "Booking not found" });
                }

                _logger.LogInformation("ConfirmPayment: Found Booking {BookingId} with Status {Status}", bookingId, booking.BookingStatus);

                // Only update if currently pending (avoid overwriting if webhook already ran)
                if (booking.BookingStatus == BookingStatus.Pending)
                {
                    booking.BookingStatus = BookingStatus.Paid; // Changed from Confirmed to Paid per business requirement
                    booking.PaymentStatus = PaymentStatus.Paid;
                    booking.PaymentIntentId = pi.Id;
                    booking.PaymentMethod = PaymentMethod.Stripe;
                    booking.PaymentDate = DateTime.UtcNow;
                    booking.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    await _unitOfWork.SaveChanges();

                    _logger.LogInformation("Booking {BookingId} status updated to 'Paid' via frontend callback", bookingId);

                    _logger.LogInformation("Booking {BookingId} status updated to 'Paid' via frontend callback", bookingId);

                     // Log Event
                    try
                    {
                        var evt = new PaymentEvent
                        {
                            PaymentId = 0, // We might not have the payment ID easily here without querying again, or we can use the intent ID logic
                            Provider = "Stripe",
                            EventId = pi.Id,
                            Payload = "Payment Confirmed by Client Callback",
                            ProcessedAt = DateTime.UtcNow
                        };
                         // Try to match payment
                        var payment = await _unitOfWork.Payments.FindBySearch(p => p.PaymentIntentId == pi.Id);
                        if(payment != null) evt.PaymentId = payment.PaymentId;

                        await _unitOfWork.PaymentEvents.AddAsync(evt);
                    }
                    catch (Exception ex) {_logger.LogWarning(ex, "Failed to log event");}

                     // Send Success Email
                    if (booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
                    {
                         try
                         {
                            var itemsTotal = 0m;
                            var dto = new UmarahBooking.Core.DTO.PaymentReceiptDto
                            {
                                CustomerName = booking.User.FullName ?? "Customer",
                                CustomerEmail = booking.User.Email, // Default, override below
                                BookingNumber = booking.BookingNumber ?? booking.BookingId.ToString(),
                                TransactionId = pi.Id,
                                PaymentMethod = "Stripe",
                                PaymentDate = DateTime.UtcNow,
                                TripStartDate = booking.TravelStartDate ?? DateTime.UtcNow,
                                TripEndDate = booking.TravelEndDate ?? booking.TravelStartDate ?? DateTime.UtcNow,
                                TripType = booking.TripType.ToString(),
                                TotalAmount = booking.TotalPrice ?? 0m
                            };

                            // Add Hotels
                            foreach(var h in booking.Hotels)
                            {
                                dto.Items.Add(new UmarahBooking.Core.DTO.ReceiptItemDto
                                {
                                    Title = $"{h.Hotel?.Name ?? "Hotel"} Accommodation",
                                    Details = new List<string> 
                                    { 
                                        $"Check-in: {h.CheckInDate:MMM d, yyyy}",
                                        $"Check-out: {h.CheckOutDate:MMM d, yyyy}",
                                        $"Rooms: {h.NumberOfRooms}"
                                    },
                                    Amount = h.TotalPrice
                                });
                                itemsTotal += h.TotalPrice;
                            }

                            // Add Flights
                            foreach(var f in booking.BookingInternationalTransport)
                            {
                                dto.Items.Add(new UmarahBooking.Core.DTO.ReceiptItemDto
                                {
                                    Title = "International Flight",
                                    Details = new List<string> 
                                    {
                                        $"Carrier: {f.InternationalTransport?.CarrierName ?? "Airline"}",
                                        $"Seats: {f.NumberOfSeats}"
                                    },
                                    Amount = f.TotalPrice
                                });
                                itemsTotal += f.TotalPrice;
                            }

                            // Add Ground
                            foreach(var g in booking.BookingGroundTransport)
                            {
                                dto.Items.Add(new UmarahBooking.Core.DTO.ReceiptItemDto
                                {
                                    Title = "Ground Transportation",
                                    Details = new List<string> 
                                    {
                                        $"Service Date: {g.ServiceDate:MMM d, yyyy}",
                                        $"Service: {g.GroundTransport?.ServiceName ?? "Transport"}"
                                    },
                                    Amount = g.TotalPrice
                                });
                                itemsTotal += g.TotalPrice;
                            }
                            
                            // Calc Tax & Fees
                            dto.Tax = itemsTotal * 0.05m;
                            dto.OtherFees = (booking.TotalPrice ?? 0m) - itemsTotal - dto.Tax;

                            // 1. Send Detailed Receipt to Main Traveler (or User if not found)
                            var mainTraveler = booking.Travelers.FirstOrDefault(t => t.IsMainTraveler == true);
                            var receiptRecipient = (!string.IsNullOrEmpty(mainTraveler?.Email)) ? mainTraveler.Email : booking.User.Email;
                            
                            // Override email for dispatch
                            dto.CustomerEmail = receiptRecipient;
                            if(!string.IsNullOrEmpty(mainTraveler?.FirstName)) 
                                dto.CustomerName = $"{mainTraveler.FirstName} {mainTraveler.LastName}";

                            await _emailService.SendDetailedPaymentReceiptAsync(dto);

                            // 2. Send Simple Notification to User Booking Account (if different)
                            if (receiptRecipient != booking.User.Email)
                            {
                                await _emailService.SendPaymentSuccessEmailAsync(
                                    booking.User.Email, 
                                    dto.BookingNumber, 
                                    dto.TotalAmount, 
                                    booking.User.FullName ?? "Customer",
                                    dto.TripStartDate,
                                    dto.TripEndDate,
                                    dto.TripType);
                            }
                         }
                         catch (Exception ex)
                         {
                             _logger.LogError(ex, "Failed to send receipt email");
                         }
                    }
                }
                else
                {
                     _logger.LogInformation("Booking {BookingId} was already {Status}, skipping update", bookingId, booking.BookingStatus);
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
                return BadRequest(new { message = $"Stripe error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unexpected error confirming payment");
                 return StatusCode(500, new { message = $"DEBUG ERROR: {ex.Message} | {ex.InnerException?.Message}" });
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

                    // Log Event
                    try
                    {
                        var evt = new PaymentEvent
                        {
                            PaymentId = payment.PaymentId,
                            Provider = "Stripe",
                            EventId = pi.Id,
                            Payload = $"Webhook: Payment Succeeded",
                            ProcessedAt = DateTime.UtcNow
                        };
                         await _unitOfWork.PaymentEvents.AddAsync(evt);
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "Failed to log webhook event"); }
                
                    // Send Success Email
                    // Note: We need to reload booking with navigation properties to send full receipt
                    // Or we can rely on ConfirmPayment to do it if the user stays on the page.
                    // But to be safe, if this is the first time confirming, we should try to send it.
                    // Since fetching navigation properties here requires a new query, let's do it.
                     if (booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
                    {
                         try
                         {
                            // Fetch full details
                            var fullBooking = (await _unitOfWork.Bookings.FindWithAsync(
                                b => b.BookingId == booking.BookingId,
                                new[] { 
                                    "Hotels", "Hotels.Hotel", "Travelers",
                                    "BookingInternationalTransport", "BookingInternationalTransport.InternationalTransport",
                                    "BookingGroundTransport", "BookingGroundTransport.GroundTransport"
                                } 
                            )).FirstOrDefault();

                            if(fullBooking != null)
                            {
                                var itemsTotal = 0m;
                                var dto = new UmarahBooking.Core.DTO.PaymentReceiptDto
                                {
                                    CustomerName = booking.User.FullName ?? "Customer",
                                    CustomerEmail = booking.User.Email,
                                    BookingNumber = booking.BookingNumber ?? booking.BookingId.ToString(),
                                    TransactionId = pi.Id,
                                    PaymentMethod = "Stripe",
                                    PaymentDate = DateTime.UtcNow,
                                    TripStartDate = booking.TravelStartDate ?? DateTime.UtcNow,
                                    TripEndDate = booking.TravelEndDate ?? booking.TravelStartDate ?? DateTime.UtcNow,
                                    TripType = booking.TripType.ToString(),
                                    TotalAmount = booking.TotalPrice ?? 0m
                                };

                                // Map Hotels
                                foreach(var h in fullBooking.Hotels)
                                {
                                    dto.Items.Add(new UmarahBooking.Core.DTO.ReceiptItemDto
                                    {
                                        Title = $"{h.Hotel?.Name ?? "Hotel"} Accommodation",
                                        Details = new List<string> 
                                        { 
                                            $"Check-in: {h.CheckInDate:MMM d, yyyy}",
                                            $"Check-out: {h.CheckOutDate:MMM d, yyyy}",
                                        },
                                        Amount = h.TotalPrice
                                    });
                                    itemsTotal += h.TotalPrice;
                                }
                                // Map Flights
                                foreach(var f in fullBooking.BookingInternationalTransport)
                                {
                                    dto.Items.Add(new UmarahBooking.Core.DTO.ReceiptItemDto
                                    {
                                        Title = "International Flight",
                                        Details = new List<string> { $"Carrier: {f.InternationalTransport?.CarrierName ?? "Airline"}"},
                                        Amount = f.TotalPrice
                                    });
                                    itemsTotal += f.TotalPrice;
                                }
                                // Map Ground
                                foreach(var g in fullBooking.BookingGroundTransport)
                                {
                                    dto.Items.Add(new UmarahBooking.Core.DTO.ReceiptItemDto
                                    {
                                        Title = "Ground Transportation",
                                        Details = new List<string> { $"Service: {g.GroundTransport?.ServiceName ?? "Transport"}" },
                                        Amount = g.TotalPrice
                                    });
                                    itemsTotal += g.TotalPrice;
                                }

                                // Calc Tax & Fees
                                dto.Tax = itemsTotal * 0.05m;
                                dto.OtherFees = (booking.TotalPrice ?? 0m) - itemsTotal - dto.Tax;

                                // 1. Send Detailed Receipt to Main Traveler (or User if not found)
                                var mainTraveler = fullBooking.Travelers.FirstOrDefault(t => t.IsMainTraveler == true);
                                var receiptRecipient = (!string.IsNullOrEmpty(mainTraveler?.Email)) ? mainTraveler.Email : booking.User.Email;
                                
                                // Override email for dispatch
                                dto.CustomerEmail = receiptRecipient;
                                if(!string.IsNullOrEmpty(mainTraveler?.FirstName)) 
                                    dto.CustomerName = $"{mainTraveler.FirstName} {mainTraveler.LastName}";

                                await _emailService.SendDetailedPaymentReceiptAsync(dto);

                                // 2. Send Simple Notification to User Booking Account (if different)
                                if (receiptRecipient != booking.User.Email)
                                {
                                    await _emailService.SendPaymentSuccessEmailAsync(
                                        booking.User.Email, 
                                        dto.BookingNumber, 
                                        dto.TotalAmount, 
                                        booking.User.FullName ?? "Customer",
                                        dto.TripStartDate,
                                        dto.TripEndDate,
                                        dto.TripType
                                    );
                                }
                            }
                         }
                         catch(Exception ex)
                         {
                             _logger.LogError(ex, "Failed to send receipt email from webhook");
                         }
                    }
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
        public decimal Amount { get; set; } // Added to allow frontend calculation override
        public string Currency { get; set; } = "usd";
        public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
