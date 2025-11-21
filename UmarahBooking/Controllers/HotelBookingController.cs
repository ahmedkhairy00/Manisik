using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

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
        //[Authorize] // change from AllowAnonymous to require login
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<HotelBookingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("BookHotel")]
        public async Task<IActionResult> BookHotel([FromBody] HotelBookingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<HotelBookingDto>.ErrorResponse("Invalid booking data"));


                //var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                //if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                //{
                //    return BadRequest(ApiResponse<HotelBookingDto>.ErrorResponse("User not found or not logged in"));
                //}
                int userId = 2;

                var bookingHotel = await _bookingService.BookHotelAsync(dto, userId);

                return Ok(ApiResponse<HotelBookingDto>.SuccessResponse(null, "Hotel booked successfully"));
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
                    "An error occurred while booking the hotel"));
            }
        }


    }
}
