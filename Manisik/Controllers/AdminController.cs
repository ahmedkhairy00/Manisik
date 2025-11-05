using Manisik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Manisik.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<Auth> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminController(UserManager<Auth> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] DTOs.RoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name)) return BadRequest("Role name is required.");

            var exists = await _roleManager.RoleExistsAsync(dto.Name);
            if (exists) return BadRequest("Role already exists.");

            var res = await _roleManager.CreateAsync(new IdentityRole<int> { Name = dto.Name });
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok();
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] DTOs.AssignRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest("Email and Role are required.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound("User not found.");

            if (!await _roleManager.RoleExistsAsync(dto.Role)) return NotFound("Role not found.");

            var res = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok();
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] DTOs.AssignRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest("Email and Role are required.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound("User not found.");

            var res = await _userManager.RemoveFromRoleAsync(user, dto.Role);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok();
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }
    }
}
