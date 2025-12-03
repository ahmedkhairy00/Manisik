using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingInternationalTransportController : ControllerBase
    {

        private readonly IInternationalTransportBookingService _bookingService;
        private readonly ILogger<BookingInternationalTransportController> _logger;

        public BookingInternationalTransportController(
            IInternationalTransportBookingService bookingService,
            ILogger<BookingInternationalTransportController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }





        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<TransportBookingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("BookInteranationalTransport")]
        public async Task<IActionResult> BookInternationalTransport([FromBody] TransportBookingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid booking data");

                // 1️⃣ Get UserId from JWT
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("User not found or not logged in");
                }

                // 2️⃣ Call Service to complete booking
                var bookingResult = await _bookingService.BookInternationalTransportAsync(userId, dto);

                return Ok("Transport booked successfully");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Transport booking validation failed");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while booking transport");
                return StatusCode(500, "Something went wrong while booking transport");
            }
        }

    }
}
