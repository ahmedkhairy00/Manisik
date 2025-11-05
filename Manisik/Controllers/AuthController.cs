using Manisik.DTOs;
using Manisik.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Manisik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(AuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto.Email, dto.Password);
            if (!result) return BadRequest("Failed to register user.");
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _authService.LoginAsync(dto.Email, dto.Password);
            if (user == null) return Unauthorized("Invalid credentials.");

            var token = await _jwtService.GenerateTokenAsync(user);
            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = string.Join(",", (await _jwtService.GetRolesForUserAsync(user)) )
            });
        }
    }
}
