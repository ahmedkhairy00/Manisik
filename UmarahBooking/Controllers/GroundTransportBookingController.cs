using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;
using System.Text.Json;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroundTransportBookingController : ControllerBase
    {
        private readonly IBookingGroundTransportService _bookingService;
        private readonly ILogger<GroundTransportBookingController> _logger;

        public GroundTransportBookingController(
            IBookingGroundTransportService bookingService, 
            ILogger<GroundTransportBookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }


        /// <summary>
        /// Book ground transport for the logged-in user
        /// </summary>
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<GroundTransportBookingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("BookGroundTransport")]
        public async Task<IActionResult> BookGroundTransport([FromBody] GroundTransportBookingDto dto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "Invalid booking data. Please check all required fields."));
                }

                // Validate DTO is not null
                if (dto == null)
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "Request body is required."));
                }

                // Early validation to catch common client issues
                if (dto.GroundTransportId <= 0)
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "GroundTransportId is required and must be greater than zero."));
                }

                if (dto.NumberOfPassengers <= 0)
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "NumberOfPassengers must be greater than zero."));
                }

                if (string.IsNullOrWhiteSpace(dto.PickupLocation))
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "PickupLocation is required."));
                }

                if (string.IsNullOrWhiteSpace(dto.DropoffLocation))
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "DropoffLocation is required."));
                }

                if (dto.ServiceDate.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "Service date must be in the future."));
                }

                // Extract and validate user authentication
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        "Authentication required. Please log in to continue."));
                }

                // Log request for debugging (optional, remove in production or use Debug level)
                _logger.LogDebug("BookGroundTransport request by user {UserId}: GroundTransportId={TransportId}, Passengers={Passengers}, ServiceDate={ServiceDate}",
                    userId, dto.GroundTransportId, dto.NumberOfPassengers, dto.ServiceDate);

                // Process booking
                var bookingGroundTransport = await _bookingService.BookGroundTransportAsync(dto, userId);

                // Build response DTO from created entity
                var responseDto = new GroundTransportBookingDto
                {
                    GroundTransportId = bookingGroundTransport.GroundTransportId,
                    ServiceName = bookingGroundTransport.GroundTransport?.ServiceName,
                    Type = bookingGroundTransport.GroundTransport?.InternalTransportType,
                    ServiceDate = bookingGroundTransport.ServiceDate,
                    PickupLocation = bookingGroundTransport.PickupLocation,
                    DropoffLocation = bookingGroundTransport.DropoffLocation,
                    NumberOfPassengers = bookingGroundTransport.NumberOfPassengers,
                    PricePerPerson = bookingGroundTransport.NumberOfPassengers > 0
                        ? bookingGroundTransport.TotalPrice / bookingGroundTransport.NumberOfPassengers
                        : 0,
                    TotalPrice = bookingGroundTransport.TotalPrice
                };

                _logger.LogInformation("Ground transport booking successful for user {UserId}, BookingGroundTransportId={BookingGroundTransportId}",
                    userId, bookingGroundTransport.BookingGroundTransportId);

                return Ok(ApiResponse<GroundTransportBookingDto>.SuccessResponse(
                    responseDto,
                    "Ground transport booking completed successfully. Your reservation has been confirmed."));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ground transport booking validation failed for GroundTransportId={TransportId}",
                    dto?.GroundTransportId);
                return BadRequest(ApiResponse<GroundTransportBookingDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while booking ground transport for GroundTransportId={TransportId}",
                    dto?.GroundTransportId);

                // Include detailed error in development mode
                var env = HttpContext.RequestServices.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>();
                if (env?.IsDevelopment() ?? false)
                {
                    return StatusCode(500, ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                        $"Unexpected error: {ex.Message}"));
                }

                return StatusCode(500, ApiResponse<GroundTransportBookingDto>.ErrorResponse(
                    "An unexpected error occurred while processing your ground transport booking. Please try again later."));
            }
        }
        /// <summary>
        /// Save pending ground transport draft
        /// </summary>
        [Authorize]
        [HttpPost("SavePending")]
        public async Task<IActionResult> SavePending([FromBody] JsonElement payload)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return BadRequest(ApiResponse<string>.ErrorResponse("Authentication required. Please log in to continue."));

            try
            {
                var createdIds = new List<int>();

                // payload may contain 'ground' or direct DTO
                if (payload.TryGetProperty("ground", out var groundProp) && groundProp.ValueKind != JsonValueKind.Null)
                {
                    var dto = JsonSerializer.Deserialize<GroundTransportBookingDto>(groundProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        var item = await _bookingService.BookGroundTransportAsync(dto, userId);
                        createdIds.Add(item.BookingGroundTransportId);
                    }
                }

                // Direct DTO
                if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("groundTransportId", out var _))
                {
                    var dto = JsonSerializer.Deserialize<GroundTransportBookingDto>(payload.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        var item = await _bookingService.BookGroundTransportAsync(dto, userId);
                        createdIds.Add(item.BookingGroundTransportId);
                    }
                }

                return Ok(ApiResponse<IEnumerable<int>>.SuccessResponse(createdIds, "Pending ground transport draft saved"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "SavePending validation failed");
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving pending ground booking");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while saving pending ground booking"));
            }
        }

        /// <summary>
        /// Get current user's pending ground transport bookings
        /// </summary>
        [Authorize]
        [HttpGet("MyPendingGroundBookings")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMyPendingGroundBookings()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Authentication required. Please log in to continue."));
                }

                var pendingBookings = await _bookingService.GetPendingGroundBookingsAsync(userId);

                return Ok(ApiResponse<object>.SuccessResponse(pendingBookings, "Pending ground transport bookings retrieved successfully"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving pending ground bookings");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving your pending bookings."));
            }
        }

        /// <summary>
        /// Delete a pending ground transport booking for the current user
        /// </summary>
        [Authorize]
        [HttpDelete("DeletePendingGroundBooking/{id:int}")]
        public async Task<IActionResult> DeletePendingGroundBooking(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return BadRequest(ApiResponse<string>.ErrorResponse("Authentication required. Please log in to continue."));

            var success = await _bookingService.DeletePendingGroundBookingAsync(id, userId);

            if (!success)
                return NotFound(ApiResponse<string>.ErrorResponse("Pending ground booking not found or you are not authorized to delete it."));

            return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Pending ground booking deleted successfully."));
        }
    }
}
