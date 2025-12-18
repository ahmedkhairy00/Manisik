using AutoMapper;
using UmarahBooking.Core.Enums;
using UmarahBooking.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All booking operations require authentication
    public class BookingController : ControllerBase
    {
        #region Dependencies

        private readonly ILogger<BookingController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IPdfGenerationService _pdfService;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public BookingController(
            ILogger<BookingController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IPdfGenerationService pdfService,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _pdfService = pdfService;
            _environment = environment;
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Get all bookings for the current user
        /// </summary>
        /// <returns>List of user's bookings</returns>
        [HttpGet("MyBookings")]
        [Authorize(Roles = "User,Admin,HotelManager")]
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
                var query = _unitOfWork.Bookings.GetAllAsQuerable()
                    .Where(b => b.UserId == userId);

                var includes = new[] { "Hotels", "Hotels.Hotel", "Hotels.Room", "Travelers", "BookingInternationalTransport", "BookingInternationalTransport.InternationalTransport", "BookingGroundTransport", "BookingGroundTransport.GroundTransport", "Payment" };

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

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
                var query = _unitOfWork.Bookings.GetAllAsQuerable();

                var includes = new[] 
                {
                    "Hotels", "Hotels.Hotel", "Hotels.Room", 
                    "Travelers", 
                    "BookingInternationalTransport", "BookingInternationalTransport.InternationalTransport",
                    "BookingGroundTransport", "BookingGroundTransport.GroundTransport", 
                    "Payment", "User"
                };

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

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
                var booking = await _unitOfWork.Bookings.FindWithAsync(
                    b => b.BookingId == id,
                    new[] { "Hotels", "Hotels.Hotel", "Hotels.Room", "Travelers", "BookingInternationalTransport", "BookingInternationalTransport.InternationalTransport", "BookingGroundTransport", "BookingGroundTransport.GroundTransport", "Payment" });
                
                var firstBooking = booking.FirstOrDefault();

                if (firstBooking == null)
                {
                    return NotFound(ApiResponse<BookingDto>.ErrorResponse(
                        $"Booking with ID {id} not found"));
                }

                // Check if user owns this booking or is admin
                var isAdmin = User.IsInRole("Admin");
                if (firstBooking.UserId != userId && !isAdmin)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to access booking {BookingId} owned by {OwnerId}",
                        userId, id, firstBooking.UserId);

                    return Forbid(); // User doesn't own this booking
                }

                var bookingDto = _mapper.Map<BookingDto>(firstBooking);

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

        // Get Booking with BookingId
        [HttpGet("BookingId/{id:int}")]
        [AllowAnonymous]

        public async Task<IActionResult> GetBookingByBookingId(int id)
        {
            var booking = await _unitOfWork.Bookings.FindBySearch(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound(ApiResponse<BookingDto>.ErrorResponse(
                        $"Booking with ID {id} not found"));
            }

            var bookingDto = _mapper.Map<BookingDto>(booking);

            return Ok(ApiResponse<BookingDto>.SuccessResponse(
                bookingDto,
                $"Booking retrieved successfully with BookingId {bookingDto.Id}"));
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

        /// <summary>
        /// Download Visa or Ticket PDF document for a specific traveler
        /// </summary>
        [HttpGet("{bookingId:int}/Documents/{travelerId:int}/{documentType}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DownloadDocument(int bookingId, int travelerId, string documentType)
        {
            try
            {
                documentType = documentType.ToLowerInvariant();
                if (documentType != "visa" && documentType != "ticket")
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Invalid document type. Use 'visa' or 'ticket'."));
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid user token"));
                }

                var bookings = await _unitOfWork.Bookings.FindWithAsync(
                    b => b.BookingId == bookingId,
                    new[] { "Travelers" });
                
                var booking = bookings.FirstOrDefault();
                if (booking == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse($"Booking with ID {bookingId} not found"));
                }

                var isAdmin = User.IsInRole("Admin");
                if (booking.UserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                if (booking.BookingStatus != BookingStatus.Confirmed)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Documents are only available for confirmed bookings"));
                }

                var traveler = booking.Travelers?.FirstOrDefault(t => t.TravelerId == travelerId);
                if (traveler == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse($"Traveler with ID {travelerId} not found"));
                }

                var fullName = $"{traveler.FirstName} {traveler.LastName}".Trim();
                byte[] pdfBytes;
                string fileName;

                if (documentType == "visa")
                {
                    byte[]? photoBytes = null;
                    _logger.LogInformation("Traveler PhotoUrl: {PhotoUrl}", traveler.PhotoUrl ?? "NULL");
                    
                    if (!string.IsNullOrEmpty(traveler.PhotoUrl))
                    {
                        try
                        {
                            var webRoot = _environment.WebRootPath ?? _environment.ContentRootPath;
                            var photoPath = Path.Combine(webRoot, traveler.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                            _logger.LogInformation("Looking for photo at: {PhotoPath}", photoPath);
                            
                            if (System.IO.File.Exists(photoPath))
                            {
                                photoBytes = await System.IO.File.ReadAllBytesAsync(photoPath);
                                _logger.LogInformation("Photo loaded successfully, size: {Size} bytes", photoBytes.Length);
                            }
                            else
                            {
                                _logger.LogWarning("Photo file not found at path: {PhotoPath}", photoPath);
                            }
                        }
                        catch (Exception photoEx)
                        {
                            _logger.LogError(photoEx, "Error loading photo from {PhotoUrl}", traveler.PhotoUrl);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Traveler {TravelerId} has no PhotoUrl set", traveler.TravelerId);
                    }

                    var visaData = new VisaPdfData
                    {
                        FullName = fullName,
                        PassportNumber = traveler.PassportNumber ?? "",
                        Nationality = traveler.Nationality ?? "",
                        DateOfBirth = traveler.DateOfBirth,
                        PhotoUrl = traveler.PhotoUrl,
                        PhotoBytes = photoBytes,
                        VisaType = "Umrah",
                        VisaExpiryDate = DateTime.UtcNow.AddDays(90),
                        StayDuration = 30,
                        EntryCount = 1,
                        IssuingAuthority = "Manisik",
                        BookingNumber = booking.BookingNumber ?? bookingId.ToString(),
                        TravelStartDate = booking.TravelStartDate ?? DateTime.UtcNow,
                        TravelEndDate = booking.TravelEndDate ?? DateTime.UtcNow.AddDays(30)
                    };
                    pdfBytes = _pdfService.GenerateVisaPdf(visaData);
                    fileName = $"Visa_{fullName.Replace(" ", "_")}.pdf";
                }
                else
                {
                    var transportBookings = await _unitOfWork.BookingInternationalTransports
                        .GetAllAsQuerable()
                        .Include(bit => bit.InternationalTransport)
                        .Where(bit => bit.BookingId == bookingId)
                        .ToListAsync();
                    
                    var transport = transportBookings.FirstOrDefault()?.InternationalTransport;

                    var ticketData = new TicketPdfData
                    {
                        FullName = fullName,
                        PassportNumber = traveler.PassportNumber ?? "",
                        Nationality = traveler.Nationality ?? "",
                        DateOfBirth = traveler.DateOfBirth,
                        VisaType = "Umrah",
                        FlightNumber = transport?.FlightNumber ?? "TBA",
                        CarrierName = transport?.CarrierName ?? "TBA",
                        DepartureAirport = transport?.DepartureAirport.ToString() ?? "TBA",
                        ArrivalAirport = transport?.ArrivalAirport.ToString() ?? "TBA",
                        DepartureDate = transport?.DepartureDate ?? booking.TravelStartDate ?? DateTime.UtcNow,
                        SeatNumber = null,
                        BookingNumber = booking.BookingNumber ?? bookingId.ToString(),
                        ReturnDate = booking.TravelEndDate,
                        ReturnFlightNumber = transport?.FlightNumber != null ? $"{transport.FlightNumber}R" : null
                    };
                    pdfBytes = _pdfService.GenerateTicketPdf(ticketData);
                    fileName = $"Ticket_{fullName.Replace(" ", "_")}.pdf";
                }

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating document");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while generating the document"));
            }
        }

        #endregion

        #region POST Operations

        /// <summary>
        /// Create a complete booking (hotels, transport, travelers)
        /// </summary>
        /// <param name="bookingDto">Complete booking details</param>
        /// <returns>Created booking</returns>
        [HttpGet("TestDebug")]
        [AllowAnonymous]
        public IActionResult TestDebug()
        {
            return Ok(new { message = "Server is running updated version V3 (GlobalHandler + Stripe Debug)" });
        }

        [HttpPost("CreateBooking")]
        [Authorize(Roles = "User,Admin")]
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
                    return Unauthorized(ApiResponse<BookingDto>.ErrorResponse("Invalid user token"));
                }

                Booking? booking = null;

                // ? NEW: Get or create pending booking
                // Strategy: If ID is provided in DTO, use it. If not, try to find latest pending. 
                // Since this is the Finalize Step, we expect a BookingId usually, but let's handle both.
                
                if (bookingDto.Id > 0) 
                {
                     booking = await _unitOfWork.Bookings.GetByIdAsync(bookingDto.Id);
                     if (booking == null || booking.UserId != userId) 
                          return NotFound(ApiResponse<BookingDto>.ErrorResponse("Booking not found or access denied"));
                } 
                else 
                {
                    booking = await _unitOfWork.Context.Set<Booking>()
                        .Where(b => b.UserId == userId && b.BookingStatus == BookingStatus.Pending)
                        .OrderByDescending(b => b.CreatedAt) // Latest
                        .FirstOrDefaultAsync();
                }

                // Ensure BookingNumber exists if we found a pending booking
                if (booking != null && string.IsNullOrEmpty(booking.BookingNumber))
                {
                    booking.BookingNumber = await GenerateBookingNumber();
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    await _unitOfWork.SaveChanges();
                }

                if (booking == null)
                {
                    // If no pending booking exists, we create one (User started creating booking from Final Step directly?? Unlikely but possible)
                    // Or maybe it expired.
                    
                    var bookingNumber = await GenerateBookingNumber();
                    booking = new Booking
                    {
                        BookingNumber = bookingNumber,
                        UserId = userId,
                        TripType = ParseTripType(bookingDto.Type),
                        BookingStatus = BookingStatus.Pending,
                        BookingDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow, 
                         // set TTL
                        ReservedUntil = DateTime.UtcNow.AddMinutes(120)
                    };

                    await _unitOfWork.Bookings.AddAsync(booking);
                    await _unitOfWork.SaveChanges(); // Need ID
                }
               
               // ... Update logic ...
                    // Update logic
                    // Trust frontend values for Price and Fee to avoid double taxation
                    booking.TripType = ParseTripType(bookingDto.Type);
                    booking.TravelStartDate = bookingDto.TravelStartDate;
                    booking.TravelEndDate = bookingDto.TravelEndDate ?? bookingDto.TravelStartDate;
                    booking.NumberOfTravelers = bookingDto.NumberOfTravelers;
                    
                    // Use DTO values directly
                    booking.ServiceFee = bookingDto.ServiceFee ?? 0; 
                    booking.TotalPrice = bookingDto.TotalPrice ?? 0;
                    
                    booking.PaymentMethod = bookingDto.PaymentMethod ?? PaymentMethod.Stripe;
                    // Keep existing payment status if it was already updated, otherwise set to Pending
                    if (booking.PaymentStatus == PaymentStatus.Pending && booking.PaymentDate == null) 
                    {
                        booking.PaymentStatus = PaymentStatus.Pending; 
                    }
                    booking.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    await _unitOfWork.SaveChanges();
               

                // ? Verify pending bookings exist (they should from earlier steps)
                var hotelBookings = await _unitOfWork.BookingHotels
                    .GetAllAsQuerable()
                    .Where(bh => bh.BookingId == booking.BookingId)
                    .ToListAsync();

                var transportBookings = await _unitOfWork.BookingInternationalTransports
                    .GetAllAsQuerable()
                    .Where(bit => bit.BookingId == booking.BookingId)
                    .ToListAsync();

                var groundBookings = await _unitOfWork.BookingGroundTransports
                    .GetAllAsQuerable()
                    .Where(bgt => bgt.BookingId == booking.BookingId)
                    .ToListAsync();

                // ? If no pending bookings found, create them from DTO (Safety net)
                if (!hotelBookings.Any())
                {
                    if (bookingDto.MakkahHotel != null)
                        await ProcessHotelBooking(booking.BookingId, bookingDto.MakkahHotel, HotelCity.Makkah);

                    if (bookingDto.MadinahHotel != null)
                        await ProcessHotelBooking(booking.BookingId, bookingDto.MadinahHotel, HotelCity.Madinah);
                }

                if (!transportBookings.Any() && bookingDto.InternationalTransport != null)
                {
                    await ProcessInternationalTransport(booking.BookingId, bookingDto.InternationalTransport);
                }

                if (!groundBookings.Any() && bookingDto.GroundTransport != null)
                {
                    await ProcessGroundTransport(booking.BookingId, bookingDto.GroundTransport);
                }

                // ? Process Travelers (always update/create)
                // Delete existing travelers for this booking - only those with valid IDs (persisted)
                var existingTravelers = await _unitOfWork.Travelers
                    .GetAllAsQuerable()
                    .Where(t => t.BookingId == booking.BookingId && t.TravelerId > 0)
                    .ToListAsync();

                if (existingTravelers.Any())
                {
                    // Use RemoveRange for efficiency
                    _unitOfWork.Context.Set<Traveler>().RemoveRange(existingTravelers);
                }

                // Add new travelers
                if (bookingDto.Travelers != null && bookingDto.Travelers.Any())
                {
                    await ProcessTravelers(booking.BookingId, bookingDto.Travelers);
                }

                await _unitOfWork.SaveChanges();

                _logger.LogInformation("Booking {BookingNumber} finalized for user {UserId}",
                    booking.BookingNumber, userId);

                // Map the booking we already have in memory (no need to re-fetch)
                var createdBookingDto = _mapper.Map<BookingDto>(booking);

                return CreatedAtAction(
                    nameof(GetBookingById),
                    new { id = booking.BookingId },
                    ApiResponse<BookingDto>.SuccessResponse(
                        createdBookingDto,
                        "Booking finalized successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking");

                // TEMPORARY DEBUGGING: Always return exception details
                var errorMsg = $"DEBUG ERROR V2: {ex.Message}";
                if (ex.InnerException != null) errorMsg += $" | Inner: {ex.InnerException.Message} | Stack: {ex.InnerException.StackTrace}";
                return StatusCode(500, ApiResponse<BookingDto>.ErrorResponse(errorMsg));
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
                // Fetch booking with User and Travelers to get emails
                var booking = await _unitOfWork.Bookings.FindWithAsync(
                    b => b.BookingId == id,
                    new[] { "User", "Travelers" });
                
                var targetBooking = booking.FirstOrDefault();

                if (targetBooking == null)
                {
                    return NotFound(ApiResponse<BookingDto>.ErrorResponse(
                        $"Booking with ID {id} not found"));
                }



                if (targetBooking.BookingStatus == status)
                {
                     // optimizing: no change
                     var dto = _mapper.Map<BookingDto>(targetBooking);
                     return Ok(ApiResponse<BookingDto>.SuccessResponse(dto, $"Booking status is already {status}"));
                }

                targetBooking.BookingStatus = status;
                targetBooking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Bookings.UpdateAsync(targetBooking);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Booking {BookingId} status updated to {Status}",
                    id, status);

                int emailsSent = 0;
                int emailsFailed = 0;
                string emailStatusMsg;
                
                // Get booking type string
                var bookingTypeStr = targetBooking.TripType.ToString();
                
                // If status is Confirmed, generate PDFs and send with attachments
                if (status == BookingStatus.Confirmed && targetBooking.Travelers != null && targetBooking.Travelers.Any())
                {
                    // Need to fetch transport details for ticket PDF
                    var transportBookings = await _unitOfWork.BookingInternationalTransports
                        .GetAllAsQuerable()
                        .Include(bit => bit.InternationalTransport)
                        .Where(bit => bit.BookingId == id)
                        .ToListAsync();
                    
                    var transport = transportBookings.FirstOrDefault()?.InternationalTransport;

                    foreach (var traveler in targetBooking.Travelers)
                    {
                        try
                        {
                            var fullName = $"{traveler.FirstName} {traveler.LastName}".Trim();
                            var attachments = new List<EmailAttachment>();

                            // Try to load photo bytes if PhotoUrl exists
                            byte[]? photoBytes = null;
                            _logger.LogInformation("Email PDF - Traveler PhotoUrl: {PhotoUrl}", traveler.PhotoUrl ?? "NULL");
                            
                            if (!string.IsNullOrEmpty(traveler.PhotoUrl))
                            {
                                try
                                {
                                    var webRoot = _environment.WebRootPath ?? _environment.ContentRootPath;
                                    var photoPath = Path.Combine(webRoot, traveler.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                                    _logger.LogInformation("Email PDF - Looking for photo at: {PhotoPath}", photoPath);
                                    
                                    if (System.IO.File.Exists(photoPath))
                                    {
                                        photoBytes = await System.IO.File.ReadAllBytesAsync(photoPath);
                                        _logger.LogInformation("Email PDF - Photo loaded, size: {Size} bytes", photoBytes.Length);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Email PDF - Photo file not found: {PhotoPath}", photoPath);
                                    }
                                }
                                catch (Exception photoEx)
                                {
                                    _logger.LogWarning(photoEx, "Failed to load photo for traveler {TravelerId}", traveler.TravelerId);
                                }
                            }

                            // Generate Visa PDF
                            var visaData = new VisaPdfData
                            {
                                FullName = fullName,
                                PassportNumber = traveler.PassportNumber ?? "",
                                Nationality = traveler.Nationality ?? "",
                                DateOfBirth = traveler.DateOfBirth,
                                PhotoUrl = traveler.PhotoUrl,
                                PhotoBytes = photoBytes,
                                VisaType = "Umrah",
                                VisaExpiryDate = DateTime.UtcNow.AddDays(90),
                                StayDuration = 30,
                                EntryCount = 1,
                                IssuingAuthority = "Manisik",
                                BookingNumber = targetBooking.BookingNumber ?? id.ToString(),
                                TravelStartDate = targetBooking.TravelStartDate ?? DateTime.UtcNow,
                                TravelEndDate = targetBooking.TravelEndDate ?? DateTime.UtcNow.AddDays(30)
                            };
                            var visaPdf = _pdfService.GenerateVisaPdf(visaData);
                            attachments.Add(new EmailAttachment
                            {
                                FileName = $"Visa_{fullName.Replace(" ", "_")}.pdf",
                                Content = visaPdf,
                                ContentType = "application/pdf"
                            });

                            // Generate Ticket PDF
                            var ticketData = new TicketPdfData
                            {
                                FullName = fullName,
                                PassportNumber = traveler.PassportNumber ?? "",
                                Nationality = traveler.Nationality ?? "",
                                DateOfBirth = traveler.DateOfBirth,
                                VisaType = "Umrah",
                                FlightNumber = transport?.FlightNumber ?? "TBA",
                                CarrierName = transport?.CarrierName ?? "TBA",
                                DepartureAirport = transport?.DepartureAirport.ToString() ?? "TBA",
                                ArrivalAirport = transport?.ArrivalAirport.ToString() ?? "TBA",
                                DepartureDate = transport?.DepartureDate ?? targetBooking.TravelStartDate ?? DateTime.UtcNow,
                                SeatNumber = null,
                                BookingNumber = targetBooking.BookingNumber ?? id.ToString(),
                                ReturnDate = targetBooking.TravelEndDate,
                                ReturnFlightNumber = transport?.FlightNumber != null ? $"{transport.FlightNumber}R" : null
                            };
                            var ticketPdf = _pdfService.GenerateTicketPdf(ticketData);
                            attachments.Add(new EmailAttachment
                            {
                                FileName = $"Ticket_{fullName.Replace(" ", "_")}.pdf",
                                Content = ticketPdf,
                                ContentType = "application/pdf"
                            });

                            // Send email with PDFs to traveler (with photo embedded in email)
                            var emailTo = !string.IsNullOrEmpty(traveler.Email) ? traveler.Email : targetBooking.User?.Email;
                            if (!string.IsNullOrEmpty(emailTo))
                            {
                                await _emailService.SendBookingConfirmedWithDocumentsAsync(
                                    emailTo,
                                    targetBooking.BookingNumber ?? id.ToString(),
                                    fullName,
                                    bookingTypeStr,
                                    attachments);
                                emailsSent++;
                                _logger.LogInformation("Sent confirmation email with PDFs to {Email} for traveler {TravelerName}", emailTo, fullName);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to generate/send PDF for traveler {TravelerId}", traveler.TravelerId);
                            emailsFailed++;
                        }
                    }
                }
                else
                {
                    // Regular status update emails (without PDFs)
                    if (!string.IsNullOrEmpty(targetBooking.User?.Email))
                    {
                        try 
                        {
                            var userName = targetBooking.User.FullName ?? "Valued Customer";
                            await _emailService.SendBookingStatusUpdateAsync(
                                targetBooking.User.Email, 
                                targetBooking.BookingNumber ?? id.ToString(), 
                                status.ToString(),
                                userName,
                                bookingTypeStr);
                            emailsSent++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send status update email to user {Email} for booking {BookingId}", targetBooking.User.Email, id);
                            emailsFailed++;
                        }
                    }
                    
                    // Send personalized emails to each traveler
                    if (targetBooking.Travelers != null)
                    {
                        foreach (var traveler in targetBooking.Travelers)
                        {
                            if (!string.IsNullOrEmpty(traveler.Email) && traveler.Email != targetBooking.User?.Email)
                            {
                                try 
                                {
                                    var travelerName = $"{traveler.FirstName} {traveler.LastName}".Trim();
                                    if (string.IsNullOrEmpty(travelerName)) travelerName = "Traveler";
                                    await _emailService.SendBookingStatusUpdateAsync(
                                        traveler.Email, 
                                        targetBooking.BookingNumber ?? id.ToString(), 
                                        status.ToString(),
                                        travelerName,
                                        bookingTypeStr);
                                    emailsSent++;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to send status update email to traveler {Email} for booking {BookingId}", traveler.Email, id);
                                    emailsFailed++;
                                }
                            }
                        }
                    }
                }
                
                if (emailsSent + emailsFailed == 0)
                {
                    emailStatusMsg = "No emails to send.";
                }
                else if (emailsFailed == 0)
                {
                    emailStatusMsg = status == BookingStatus.Confirmed 
                        ? $"Email with documents sent to {emailsSent} traveler(s)."
                        : $"Email sent to {emailsSent} recipient(s).";
                }
                else if (emailsSent == 0)
                {
                    emailStatusMsg = $"Failed to send emails to {emailsFailed} recipient(s).";
                }
                else
                {
                    emailStatusMsg = $"Email sent to {emailsSent}, failed for {emailsFailed}.";
                }

                var bookingDto = _mapper.Map<BookingDto>(targetBooking);
                return Ok(ApiResponse<BookingDto>.SuccessResponse(
                    bookingDto,
                    $"Booking status updated to {status}. {emailStatusMsg}"));
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

                    // Fetch user email if not present
                    if (booking.User == null)
                    {
                         var user = await _unitOfWork.Context.Set<ApplicationUser>().FindAsync(booking.UserId); 
                         booking.User = user;
                    }

                    if (booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
                    {
                        var amt = booking.TotalPrice ?? 0;
                        _ = _emailService.SendPaymentSuccessEmailAsync(
                            booking.User.Email, 
                            booking.BookingNumber ?? booking.BookingId.ToString(), 
                            amt,
                            booking.User.FullName ?? "Customer",
                            booking.TravelStartDate ?? DateTime.UtcNow,
                            booking.TravelEndDate ?? booking.TravelStartDate ?? DateTime.UtcNow,
                            booking.TripType.ToString());
                    }
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

                // ? Send Email Notification for Cancellation
                // Need to fetch user if not loaded (GetByIdAsync might not include it depending on repo implementation)
                // Safest to try loading it or relying on LazyLoading if enabled (but simpler to just check)
                
                string? emailToNotify = null;
                if (booking.User != null) 
                {
                    emailToNotify = booking.User.Email;
                }
                else
                {
                   var user = await _unitOfWork.Context.Set<ApplicationUser>().FindAsync(booking.UserId);
                   emailToNotify = user?.Email;
                }

                try
                {
                    if (!string.IsNullOrEmpty(emailToNotify))
                    {
                         var userName = booking.User?.FullName ?? "Valued Customer";
                         var bookingTypeStr = booking.TripType.ToString();
                         await _emailService.SendBookingStatusUpdateAsync(
                             emailToNotify, 
                             booking.BookingNumber ?? id.ToString(), 
                             "Cancelled",
                             userName,
                             bookingTypeStr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation email for booking {BookingId}", id);
                    // We don't fail the request, just log it
                }


                return Ok(ApiResponse<string>.SuccessResponse(
                    string.Empty,
                    $"Booking {booking.BookingNumber} cancelled successfully. Your reservation has been removed."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling booking");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    "An error occurred while cancelling the booking"));
            }
        }

        /// <summary>
        /// Delete booking permanently (Admin only)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("DeleteBooking/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

                if (booking == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        $"Booking with ID {id} not found"));
                }

                // Hard delete the booking
                await _unitOfWork.Bookings.DeleteAsync(booking);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Booking {BookingNumber} (ID: {BookingId}) permanently deleted by admin",
                    booking.BookingNumber, id);

                return Ok(ApiResponse<string>.SuccessResponse(
                    string.Empty,
                    $"Booking {booking.BookingNumber ?? id.ToString()} has been permanently deleted."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting booking");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    "An error occurred while deleting the booking"));
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
                b => !string.IsNullOrEmpty(b.BookingNumber) && b.BookingNumber.StartsWith(prefix));

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
                InternationalTransportId = transportDto.TransportId ?? 0,
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

        private TripType ParseTripType(string typeStr)
        {
            if (string.IsNullOrWhiteSpace(typeStr))
                return TripType.Umrah; // Default

            // Try parse as int first (e.g. "0", "1")
            if (int.TryParse(typeStr, out int typeInt))
            {
                if (Enum.IsDefined(typeof(TripType), typeInt))
                    return (TripType)typeInt;
            }

            // Try parse as string (e.g. "Umrah", "Hajj")
            if (Enum.TryParse<TripType>(typeStr, true, out var result))
            {
                return result;
            }

            return TripType.Umrah; // Default fallback
        }

        #endregion
    }
}
