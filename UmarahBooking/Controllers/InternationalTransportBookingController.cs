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
    public class InternationalTransportBookingController : ControllerBase
    {
        private readonly IBookingInternationalTransportService _bookingService;
        private readonly ILogger<InternationalTransportBookingController> _logger;

        public InternationalTransportBookingController(
            IBookingInternationalTransportService bookingService, 
            ILogger<InternationalTransportBookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Book international transport for the logged-in user
        /// </summary>
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<InternationalTransportBookingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("BookInternationalTransport")]
        public async Task<IActionResult> BookInternationalTransport([FromBody] InternationalTransportBookingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<InternationalTransportBookingDto>.ErrorResponse("Invalid booking data. Please check all required fields."));
                }

                if (dto == null)
                {
                    return BadRequest(ApiResponse<InternationalTransportBookingDto>.ErrorResponse("Request body is required."));
                }

                // Basic validation to catch common client issues early
                if (!dto.TransportId.HasValue)
                    return BadRequest(ApiResponse<InternationalTransportBookingDto>.ErrorResponse("TransportId is required."));

                if (dto.NumberOfSeats <= 0)
                    return BadRequest(ApiResponse<InternationalTransportBookingDto>.ErrorResponse("NumberOfSeats must be greater than zero."));

                if (dto.DepartureDate.HasValue && dto.DepartureDate.Value.Date < DateTime.UtcNow.Date)
                    return BadRequest(ApiResponse<InternationalTransportBookingDto>.ErrorResponse("Departure date must be in the future."));

                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<InternationalTransportBookingDto>.ErrorResponse("Authentication required. Please log in to continue."));
                }

                // Log request payload at Debug level to help troubleshoot 500s
                try
                {
                    _logger.LogDebug("BookInternationalTransport request by user {UserId}: {Dto}", userId, JsonSerializer.Serialize(dto));
                }
                catch { /* swallow logging exceptions */ }

                var bookingInternationalTransport = await _bookingService.BookInternationalTransportAsync(dto, userId);

                // Build a friendly response DTO from the created entity
                var responseDto = new InternationalTransportBookingDto
                {
                    TransportId = bookingInternationalTransport.InternationalTransportId,
                    CarrierName = bookingInternationalTransport.InternationalTransport?.CarrierName,
                    NumberOfSeats = bookingInternationalTransport.NumberOfSeats,
                    DepartureDate = bookingInternationalTransport.InternationalTransport?.DepartureDate,
                    PricePerSeat = bookingInternationalTransport.NumberOfSeats > 0
                        ? bookingInternationalTransport.TotalPrice / bookingInternationalTransport.NumberOfSeats
                        : bookingInternationalTransport.TotalPrice
                };

                return Ok(ApiResponse<InternationalTransportBookingDto>.SuccessResponse(responseDto, "International transport booking completed successfully. Your flight/ship reservation has been confirmed."));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "International transport booking validation failed");
                return BadRequest(ApiResponse<InternationalTransportBookingDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while booking international transport");

                var env = HttpContext.RequestServices.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>();
                if (env?.IsDevelopment() ?? false)
                {
                    // Include error message in development to help debugging
                    return StatusCode(500, ApiResponse<InternationalTransportBookingDto>.ErrorResponse($"Unexpected error: {ex.Message}"));
                }

                return StatusCode(500, ApiResponse<InternationalTransportBookingDto>.ErrorResponse(
                    "An unexpected error occurred while processing your international transport booking. Please try again later."));
            }
        }

        /// <summary>
        /// Save pending international transport draft
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

                // payload may contain 'transport' or direct DTO
                if (payload.TryGetProperty("transport", out var tProp) && tProp.ValueKind != JsonValueKind.Null)
                {
                    var dto = JsonSerializer.Deserialize<InternationalTransportBookingDto>(tProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        var item = await _bookingService.BookInternationalTransportAsync(dto, userId);
                        createdIds.Add(item.BookingInternationalTransportId);
                    }
                }

                // Direct DTO
                if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("transportId", out var _))
                {
                    var dto = JsonSerializer.Deserialize<InternationalTransportBookingDto>(payload.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        var item = await _bookingService.BookInternationalTransportAsync(dto, userId);
                        createdIds.Add(item.BookingInternationalTransportId);
                    }
                }

                return Ok(ApiResponse<IEnumerable<int>>.SuccessResponse(createdIds, "Pending international transport draft saved"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "SavePending validation failed");
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving pending transport booking");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while saving pending transport booking"));
            }
        }

        /// <summary>
        /// Get current user's pending international transport bookings
        /// </summary>
        [Authorize]
        [HttpGet("MyPendingTransportBookings")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMyPendingTransportBookings()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Authentication required. Please log in to continue."));
                }

                var pendingBookings = await _bookingService.GetPendingTransportBookingsAsync(userId);

                return Ok(ApiResponse<object>.SuccessResponse(pendingBookings, "Pending international transport bookings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving pending transport bookings");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving your pending bookings."));
            }
        }

        /// <summary>
        /// Delete a pending international transport booking for the current user
        /// </summary>
        [Authorize]
        [HttpDelete("DeletePendingInternationalBooking/{id:int}")]
        public async Task<IActionResult> DeletePendingInternationalBooking(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return BadRequest(ApiResponse<string>.ErrorResponse("Authentication required. Please log in to continue."));

            var success = await _bookingService.DeletePendingInternationalBookingAsync(id, userId);

            if (!success)
                return NotFound(ApiResponse<string>.ErrorResponse("Pending international booking not found or you are not authorized to delete it."));

            return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Pending international booking deleted successfully."));
        }
    }
}
