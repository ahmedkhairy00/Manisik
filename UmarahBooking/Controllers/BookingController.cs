using AutoMapper;
using Manisik.Enums;
using Manisik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Controllers
{
    /// <summary>
    /// Controller for managing complete booking operations (hotels, transport, travelers)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All booking operations require authentication
    public class BookingController : ControllerBase
    {
        #region Dependencies

        private readonly ILogger<BookingController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public BookingController(
            ILogger<BookingController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Get all bookings for the current user
        /// </summary>
        /// <returns>List of user's bookings</returns>
        [HttpGet("MyBookings")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingDto>>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                // Get current user ID from JWT claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<IEnumerable<BookingDto>>.ErrorResponse(
                        "Invalid user token"));
                }

                // Fetch user's bookings with related data
                var bookings = await _unitOfWork.Bookings.FindAllBySearch(
                    b => b.UserId == userId);

                // Map to DTOs
                var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);

                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResponse(
                    bookingDtos,
                    $"{bookings.Count()} bookings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user bookings");
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResponse(
                    "An error occurred while retrieving your bookings"));
            }
        }

        /// <summary>
        /// Get all bookings (Admin only)
        /// </summary>
        /// <returns>List of all bookings</returns>
        [HttpGet("AllBookings")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingDto>>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllBookings()
        {
            try
            {
                // Fetch all bookings with related data
                var bookings = await _unitOfWork.Bookings.FindWithAsync(new[]
                {
                    "Hotels", "Travelers", "BookingInternationalTransport",
                    "BookingGroundTransport", "Payment", "User"
                });

                var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);

                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResponse(
                    bookingDtos,
                    $"{bookings.Count()} bookings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all bookings");
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResponse(
                    "An error occurred while retrieving bookings"));
            }
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details</returns>
        [HttpGet("GetBooking/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBookingById(int id)
        {
            try
            {
                // Get current user ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<BookingDto>.ErrorResponse(
                        "Invalid user token"));
                }

                // Fetch booking with related data
                var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingDto>.ErrorResponse(
                        $"Booking with ID {id} not found"));
                }

                // Check if user owns this booking or is admin
                var isAdmin = User.IsInRole("Admin");
                if (booking.UserId != userId && !isAdmin)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to access booking {BookingId} owned by {OwnerId}",
                        userId, id, booking.UserId);

                    return Forbid(); // User doesn't own this booking
                }

                var bookingDto = _mapper.Map<BookingDto>(booking);

                return Ok(ApiResponse<BookingDto>.SuccessResponse(
                    bookingDto,
                    "Booking retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving booking {BookingId}", id);
                return StatusCode(500, ApiResponse<BookingDto>.ErrorResponse(
                    "An error occurred while retrieving the booking"));
            }
        }

        /// <summary>
        /// Search bookings by status (Admin only)
        /// </summary>
        /// <param name="status">Booking status</param>
        /// <returns>List of bookings with specified status</returns>
        [HttpGet("SearchByStatus")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingDto>>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByStatus([FromQuery] BookingStatus status)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.FindAllBySearch(
                    b => b.BookingStatus == status);

                if (!bookings.Any())
                {
                    return NotFound(ApiResponse<IEnumerable<BookingDto>>.ErrorResponse(
                        $"No bookings found with status {status}"));
                }

                var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);

                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResponse(
                    bookingDtos,
                    $"{bookings.Count()} bookings found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching bookings by status");
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResponse(
                    "An error occurred while searching bookings"));
            }
        }

        #endregion

        #region POST Operations

        /// <summary>
        /// Create a complete booking (hotels, transport, travelers)
        /// </summary>
        /// <param name="bookingDto">Complete booking details</param>
        /// <returns>Created booking</returns>
        [HttpPost("CreateBooking")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateBooking([FromBody] BookingDto bookingDto)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<BookingDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Get current user ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<BookingDto>.ErrorResponse(
                        "Invalid user token"));
                }

                // Business validation: Check if booking is complete
                if (!bookingDto.IsComplete())
                {
                    return BadRequest(ApiResponse<BookingDto>.ErrorResponse(
                        $"Booking is incomplete. You are at step {bookingDto.GetCurrentStep()} of 6"));
                }

                // Generate unique booking number
                var bookingNumber = await GenerateBookingNumber();

                // Create booking entity
                var booking = new Booking
                {
                    BookingNumber = bookingNumber,
                    UserId = userId,
                    TripType = bookingDto.Type,
                    BookingStatus = BookingStatus.Pending,
                    TravelStartDate = bookingDto.TravelStartDate,
                    TravelEndDate = bookingDto.TravelEndDate ?? bookingDto.TravelStartDate,
                    NumberOfTravelers = bookingDto.NumberOfTravelers,
                    TotalPrice = bookingDto.TotalPrice ?? 0,
                    PaymentStatus = PaymentStatus.Pending,
                    PaymentMethod = bookingDto.PaymentMethod ?? PaymentMethod.Stripe,
                    BookingDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Save booking to get ID
                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChanges();

                // Process Makkah Hotel
                if (bookingDto.MakkahHotel != null)
                {
                    await ProcessHotelBooking(booking.BookingId, bookingDto.MakkahHotel, HotelCity.Makkah);
                }

                // Process Madinah Hotel
                if (bookingDto.MadinahHotel != null)
                {
                    await ProcessHotelBooking(booking.BookingId, bookingDto.MadinahHotel, HotelCity.Madinah);
                }

                // Process International Transport
                if (bookingDto.InternationalTransport != null)
                {
                    await ProcessInternationalTransport(booking.BookingId, bookingDto.InternationalTransport);
                }

                // Process Ground Transport
                if (bookingDto.GroundTransport != null)
                {
                    await ProcessGroundTransport(booking.BookingId, bookingDto.GroundTransport);
                }

                // Process Travelers
                if (bookingDto.Travelers != null && bookingDto.Travelers.Any())
                {
                    await ProcessTravelers(booking.BookingId, bookingDto.Travelers);
                }

                // Save all changes
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Booking {BookingNumber} created successfully for user {UserId}",
                    bookingNumber, userId);

                // Reload booking with all related data
                var createdBooking = await _unitOfWork.Bookings.GetByIdAsync(booking.BookingId);
                var createdBookingDto = _mapper.Map<BookingDto>(createdBooking);

                return CreatedAtAction(
                    nameof(GetBookingById),
                    new { id = booking.BookingId },
                    ApiResponse<BookingDto>.SuccessResponse(
                        createdBookingDto,
                        "Booking created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking");
                return StatusCode(500, ApiResponse<BookingDto>.ErrorResponse(
                    "An error occurred while creating the booking"));
            }
        }

        #endregion

        #region PUT Operations

        /// <summary>
        /// Update booking status (Admin only)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="status">New status</param>
        /// <returns>Updated booking</returns>
        [HttpPut("UpdateStatus/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] BookingStatus status)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingDto>.ErrorResponse(
                        $"Booking with ID {id} not found"));
                }

                booking.BookingStatus = status;
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Booking {BookingId} status updated to {Status}",
                    id, status);

                var bookingDto = _mapper.Map<BookingDto>(booking);
                return Ok(ApiResponse<BookingDto>.SuccessResponse(
                    bookingDto,
                    $"Booking status updated to {status}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating booking status");
                return StatusCode(500, ApiResponse<BookingDto>.ErrorResponse(
                    "An error occurred while updating the booking"));
            }
        }

        /// <summary>
        /// Update payment status (Admin only)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="paymentStatus">New payment status</param>
        /// <returns>Updated booking</returns>
        [HttpPut("UpdatePaymentStatus/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentStatus paymentStatus)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingDto>.ErrorResponse(
                        $"Booking with ID {id} not found"));
                }

                booking.PaymentStatus = paymentStatus;
                booking.UpdatedAt = DateTime.UtcNow;

                // If payment is completed, update booking status
                if (paymentStatus == PaymentStatus.Paid)
                {
                    booking.BookingStatus = BookingStatus.Confirmed;
                    booking.PaymentDate = DateTime.UtcNow;
                }

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Booking {BookingId} payment status updated to {PaymentStatus}",
                    id, paymentStatus);

                var bookingDto = _mapper.Map<BookingDto>(booking);
                return Ok(ApiResponse<BookingDto>.SuccessResponse(
                    bookingDto,
                    $"Payment status updated to {paymentStatus}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating payment status");
                return StatusCode(500, ApiResponse<BookingDto>.ErrorResponse(
                    "An error occurred while updating the payment status"));
            }
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Cancel booking (User can cancel their own, Admin can cancel any)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("CancelBooking/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                // Get current user ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResponse(
                        "Invalid user token"));
                }

                var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

                if (booking == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        $"Booking with ID {id} not found"));
                }

                // Check ownership
                var isAdmin = User.IsInRole("Admin");
                if (booking.UserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                // Soft delete (change status to cancelled)
                booking.BookingStatus = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Booking {BookingNumber} cancelled by user {UserId}",
                    booking.BookingNumber, userId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    null,
                    $"Booking {booking.BookingNumber} cancelled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling booking");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    "An error occurred while cancelling the booking"));
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates unique booking number (BK-YYYY-XXXX format)
        /// </summary>
        private async Task<string> GenerateBookingNumber()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"BK-{year}-";

            // Get count of bookings this year
            var bookingsThisYear = await _unitOfWork.Bookings.FindAllBySearch(
                b => b.BookingNumber.StartsWith(prefix));

            var count = bookingsThisYear.Count() + 1;
            return $"{prefix}{count:D4}"; // BK-2025-0001
        }

        /// <summary>
        /// Process hotel booking
        /// </summary>
        private async Task ProcessHotelBooking(int bookingId, HotelBookingDto hotelDto, HotelCity city)
        {
            var bookingHotel = new BookingHotel
            {
                BookingId = bookingId,
                HotelId = hotelDto.HotelId,
                RoomId = hotelDto.RoomId,
                City = city,
                CheckInDate = hotelDto.CheckInDate,
                CheckOutDate = hotelDto.CheckOutDate,
                NumberOfRooms = hotelDto.NumberOfRooms,
                TotalPrice = hotelDto.TotalPrice ?? 0
            };

            await _unitOfWork.BookingHotels.AddAsync(bookingHotel);
        }

        /// <summary>
        /// Process international transport booking
        /// </summary>
        private async Task ProcessInternationalTransport(
            int bookingId,
            TransportBookingDto transportDto)
        {
            var bookingTransport = new BookingInternationalTransport
            {
                BookingId = bookingId,
                InternationalTransportId = transportDto.TransportId,
                NumberOfSeats = transportDto.NumberOfSeats,
                TotalPrice = transportDto.TotalPrice ?? 0
            };

            await _unitOfWork.BookingInternationalTransports.AddAsync(bookingTransport);
        }

        /// <summary>
        /// Process ground transport booking
        /// </summary>
        private async Task ProcessGroundTransport(
            int bookingId,
            GroundTransportBookingDto groundDto)
        {
            var bookingGround = new BookingGroundTransport
            {
                BookingId = bookingId,
                GroundTransportId = groundDto.GroundTransportId,
                ServiceDate = groundDto.ServiceDate,
                PickupLocation = groundDto.PickupLocation,
                DropoffLocation = groundDto.DropoffLocation,
                NumberOfPassengers = groundDto.NumberOfPassengers,
                TotalPrice = groundDto.TotalPrice ?? 0
            };

            await _unitOfWork.BookingGroundTransports.AddAsync(bookingGround);
        }

        /// <summary>
        /// Process travelers
        /// </summary>
        private async Task ProcessTravelers(int bookingId, List<TravelerDto> travelerDtos)
        {
            foreach (var travelerDto in travelerDtos)
            {
                var traveler = _mapper.Map<Traveler>(travelerDto);
                traveler.BookingId = bookingId;
                await _unitOfWork.Travelers.AddAsync(traveler);
            }
        }

        #endregion
    }
}