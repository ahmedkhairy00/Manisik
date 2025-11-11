using Manisik.DTOs;
using Manisik.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manisik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly TransportService _transportService;

        public TransportController(TransportService transportService)
        {
            _transportService = transportService;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var transports = await _transportService.GetAllAsync();
            return Ok(transports);
        }

        [HttpGet("byid/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transport = await _transportService.GetByIdAsync(id);
            if (transport == null)
                return NotFound(new { Message = "Transport not found." });

            return Ok(transport);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TransportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transportService.AddAsync(dto);
            return Ok(new
            {
                Message = "Transport added successfully.",
                Transport = result
            });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransportDto dto)
        {
            var result = await _transportService.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(new { Message = "Transport not found." });

            return Ok(new
            {
                Message = "Transport updated successfully.",
                Transport = result
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _transportService.DeleteAsync(id);
            if (!success)
                return NotFound(new { Message = "Transport not found." });

            return Ok(new { Message = "Transport deleted successfully." });
        }
    }
}
