using Manisik.DTOs;
using Manisik.Services;
using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _bookingService.GetAllAsync();
            return Ok(bookings);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null) return NotFound();

            return Ok(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UmrahBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bookingService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.UmrahBookingId }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UmrahBookingDto dto)
        {
            var result = await _bookingService.UpdateAsync(id, dto);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bookingService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
