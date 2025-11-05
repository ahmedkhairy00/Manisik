using Manisik.DTOs;
using Manisik.Services;
using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transports = await _transportService.GetAllAsync();
            return Ok(transports);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transport = await _transportService.GetByIdAsync(id);
            if (transport == null) return NotFound();

            return Ok(transport);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transportService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.TransportId }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransportDto dto)
        {
            var result = await _transportService.UpdateAsync(id, dto);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _transportService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
