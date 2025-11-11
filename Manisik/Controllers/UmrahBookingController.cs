using Manisik.DTOs;
using Manisik.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manisik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UmrahBookingController : ControllerBase
    {
        private readonly UmrahBookingService _bookingService;

        public UmrahBookingController(UmrahBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _bookingService.GetAllAsync();
            return Ok(bookings);
        }

        [HttpGet("byid/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
                return NotFound(new { Message = "Booking not found." });

            return Ok(booking);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UmrahBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bookingService.AddAsync(dto);
            return Ok(new
            {
                Message = "Booking created successfully.",
                Booking = result
            });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UmrahBookingDto dto)
        {
            var result = await _bookingService.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(new { Message = "Booking not found." });

            return Ok(new
            {
                Message = "Booking updated successfully.",
                Booking = result
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bookingService.DeleteAsync(id);
            if (!success)
                return NotFound(new { Message = "Booking not found." });

            return Ok(new { Message = "Booking deleted successfully." });
        }
    }
}
