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
    public class HotelBookingController : ControllerBase
    {
        private readonly IBookingHotelService _bookingService;
        private readonly ILogger<HotelBookingController> _logger;

        public HotelBookingController(IBookingHotelService bookingService, ILogger<HotelBookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }
        /// <summary>
        /// Book a hotel room for the logged-in user
        /// </summary>
        [Authorize] // change from AllowAnonymous to require login
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<HotelBookingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("BookHotel")]
        public async Task<IActionResult> BookHotel([FromBody] HotelBookingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<HotelBookingDto>.ErrorResponse("Invalid booking data. Please check all required fields."));


                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<HotelBookingDto>.ErrorResponse("Authentication required. Please log in to continue."));
                }


                var bookingHotel = await _bookingService.BookHotelAsync(dto, userId);

                return Ok(ApiResponse<HotelBookingDto>.SuccessResponse(new HotelBookingDto(), "Hotel booking completed successfully. Your room reservation has been confirmed."));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Booking validation failed");
                return BadRequest(ApiResponse<HotelBookingDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while booking hotel");
                return StatusCode(500, ApiResponse<HotelBookingDto>.ErrorResponse(
                    "An unexpected error occurred while processing your hotel booking. Please try again later."));
            }
        }

        /// <summary>
        /// Save a pending draft coming from frontend. Accepts a generic payload and will create pending hotel bookings
        /// for any `makkahHotel` / `madinahHotel` objects found in the payload.
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

                // Check for makkahHotel
                if (payload.TryGetProperty("makkahHotel", out var makkahProp) && makkahProp.ValueKind != JsonValueKind.Null)
                {
                    var dto = JsonSerializer.Deserialize<HotelBookingDto>(makkahProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        var bh = await _bookingService.BookHotelAsync(dto, userId);
                        createdIds.Add(bh.BookingHotelId);
                    }
                }

                // Check for madinahHotel
                if (payload.TryGetProperty("madinahHotel", out var madinahProp) && madinahProp.ValueKind != JsonValueKind.Null)
                {
                    var dto = JsonSerializer.Deserialize<HotelBookingDto>(madinahProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        var bh = await _bookingService.BookHotelAsync(dto, userId);
                        createdIds.Add(bh.BookingHotelId);
                    }
                }

                // Also allow full payload to be a direct hotel DTO
                if ((payload.ValueKind == JsonValueKind.Object) && payload.TryGetProperty("hotelId", out var _) && payload.TryGetProperty("roomId", out var _2))
                {
                    var dto = JsonSerializer.Deserialize<HotelBookingDto>(payload.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        var bh = await _bookingService.BookHotelAsync(dto, userId);
                        createdIds.Add(bh.BookingHotelId);
                    }
                }

                return Ok(ApiResponse<IEnumerable<int>>.SuccessResponse(createdIds, "Pending hotel draft saved"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "SavePending validation failed");
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving pending hotel booking");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while saving pending hotel booking"));
            }
        }

        /// <summary>
        /// Get current user's pending hotel bookings (Makkah/Madinah)
        /// </summary>
        [Authorize]
        [HttpGet("MyPendingHotelBookings")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PendingHotelBookingDto>>), 200)]
        public async Task<IActionResult> GetMyPendingHotelBookings()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<IEnumerable<PendingHotelBookingDto>>.ErrorResponse("Authentication required. Please log in to continue."));
                }

                var pendingBookings = await _bookingService.GetPendingHotelBookingsAsync(userId);

                return Ok(ApiResponse<IEnumerable<PendingHotelBookingDto>>.SuccessResponse(pendingBookings, "Pending hotel bookings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving pending hotel bookings");
                return StatusCode(500, ApiResponse<IEnumerable<PendingHotelBookingDto>>.ErrorResponse("An error occurred while retrieving your pending bookings."));
            }
        }


        /// <summary>
        /// Delete a pending hotel booking for the current user
        /// </summary>
        [Authorize]
        [HttpDelete("DeletePendingHotelBooking/{id:int}")]
        public async Task<IActionResult> DeletePendingHotelBooking(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return BadRequest(ApiResponse<string>.ErrorResponse("Authentication required. Please log in to continue."));

            var success = await _bookingService.DeletePendingHotelBookingAsync(id, userId);

            if (!success)
                return NotFound(ApiResponse<string>.ErrorResponse("Pending hotel booking not found or you are not authorized to delete it."));

            return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Pending hotel booking deleted successfully."));
        }

        
    }
}