using Manisik.DTOs;
using Manisik.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manisik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly HotelService _hotelService;

        public HotelController(HotelService hotelService)
        {
            _hotelService = hotelService;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var hotels = await _hotelService.GetAllAsync();
            return Ok(hotels);
        }

        [HttpGet("getallFiltered")]
        public async Task<IActionResult> GetAllFiltered([FromQuery] string? city, [FromQuery] string? filter)
        {
            var hotels = await _hotelService.GetFilteredHotelsAsync(city, filter);

            if (hotels == null)
                return NotFound(new { message = "No hotels found" });

            return Ok(hotels);
        }


        // ✅ Get all hotels with price filter
        [HttpGet("getallWithPriceOrder")]
        public async Task<IActionResult> GetAllWithPriceFilter(bool ascending)
        {
            var hotels = await _hotelService.GetHotelsByPriceFilterAsync(ascending);
            return Ok(hotels);
        }

        // ✅ Get all hotels with price filter
        [HttpGet("getallWithDistanceOrder")]
        public async Task<IActionResult> GetAllWithDistanceFilter(bool ascending)
        {
            var hotels = await _hotelService.GetHotelsByDistanceFilterAsync(ascending);
            return Ok(hotels);
        }

        // ✅ Get hotel by ID
        [HttpGet("byid/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
                return NotFound(new { message = "❌ Hotel not found" });

            return Ok(hotel);
        }

        // ✅ Get hotel by City name
        [HttpGet("byCity/{city:alpha}")]
        public async Task<IActionResult> GetByCityName([FromRoute] string city)
        {
            var hotels = await _hotelService.GetHotelsByCityAsync(city);
            if (hotels == null)
                return NotFound(new { message = "❌ Hotel not found" });

            return Ok(hotels);
        }

        // ✅ Create new hotel
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] HotelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _hotelService.AddAsync(dto);
            return Ok(new
            {
                message = "✅ Hotel has been added successfully",
                data = result
            });
        }

        // ✅ Update existing hotel
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HotelDto dto)
        {
            var result = await _hotelService.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(new { message = "❌ Hotel not found" });

            return Ok(new
            {
                message = "✅ Hotel has been updated successfully",
                data = result
            });
        }

        // ✅ Delete hotel
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _hotelService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "❌ Hotel not found" });

            return Ok(new { message = "✅ Hotel has been deleted successfully" });
        }
    }
}
