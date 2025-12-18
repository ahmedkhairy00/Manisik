using AutoMapper;
using UmarahBooking.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IMapper mapper,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
        }

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
            public const string HotelManager = "HotelManager";
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", errors));
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("User with this email already exists"));

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                PhoneNumber = model.PhoneNumber,
                Country = model.Country,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Failed to create user", errors));
            }

            await _userManager.AddToRoleAsync(user, Roles.User);
            _logger.LogInformation("New user registered: {Email}", user.Email);

            var token = await GenerateJwtToken(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
                Roles = new List<string> { Roles.User },
                IsActive = true,
                CreatedAt = user.CreatedAt
            };

            var expiresAt = DateTime.UtcNow.AddHours(24);

            // Set auth cookie so SPA can use cookie-based auth as well as header-based
            // Use SameSite=None to allow cross-site requests from the frontend and allow localhost to remain usable in development
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps || (Request.Host.Host?.Contains("localhost") ?? false),
                SameSite = SameSiteMode.None,
                Expires = expiresAt
            };

            Response.Cookies.Append("authToken", token, cookieOptions);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = 24 * 60 * 60,
                RefreshToken = null,
                ExpiresAt = expiresAt,
                User = userDto
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "User registered successfully"));
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", errors));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", model.Email);
                return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed login for {Email}", model.Email);
                return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password"));
            }

            var expiryDays = model.RememberMe ? 7 : 1;
            var expiresAt = DateTime.UtcNow.AddDays(expiryDays);
            var token = await GenerateJwtToken(user, expiresAt);

            // Use SameSite=None so cookie is sent by browser on cross-site requests (SPA on different origin)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps || (Request.Host.Host?.Contains("localhost") ?? false),
                SameSite = SameSiteMode.None,
                Expires = expiresAt
            };

            Response.Cookies.Append("authToken", token, cookieOptions);

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            var authResponse = new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds,
                ExpiresAt = expiresAt,
                User = userDto
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful"));
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user from Identity
            await _signInManager.SignOutAsync();

            // Delete the auth cookie if it exists
            if (Request.Cookies.ContainsKey("authToken"))
            {
                Response.Cookies.Delete("authToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps || (Request.Host.Host?.Contains("localhost") ?? false),
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });
            }

            _logger.LogInformation("User logged out successfully");

            return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out successfully"));
        }
        

        [HttpPost("AssignRole")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed"));

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            if (!IsValidRole(model.RoleName))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid role"));

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to assign role"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Role assigned successfully"));
        }

        [HttpPost("RemoveRole")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed"));

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to remove role"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Role removed successfully"));
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

            var roles = await _userManager.GetRolesAsync(user);
            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = roles.ToList();

            return Ok(ApiResponse<UserDto>.SuccessResponse(dto));
        }

        [HttpGet("Users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var dto = _mapper.Map<UserDto>(user);
                dto.Roles = roles.ToList();
                list.Add(dto);
            }

            return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(list));
        }

        [HttpGet("UsersByRole/{roleName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            if (!IsValidRole(roleName))
                return BadRequest(ApiResponse<IEnumerable<UserDto>>.ErrorResponse("Invalid role"));

            var users = await _userManager.GetUsersInRoleAsync(roleName);
            var list = users.Select(u =>
            {
                var dto = _mapper.Map<UserDto>(u);
                dto.Roles = new List<string> { roleName };
                return dto;
            }).ToList();

            return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(list));
        }

        [HttpGet("MyBookings")]
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound(ApiResponse<UserWithBookingsDto>.ErrorResponse("User not found"));

            var userWithBookings = await _uow.Users.GetAllAsQuerable()
                .Where(u => u.Id == user.Id)
                .Include(u => u.Bookings)
                    .ThenInclude(b => b.Hotels)
                        .ThenInclude(bh => bh.Hotel)
                .Include(u => u.Bookings)
                    .ThenInclude(b => b.Travelers)
                .Include(u => u.Bookings)
                    .ThenInclude(b => b.Payment)
                .FirstOrDefaultAsync();

            if (userWithBookings == null)
            {
                var empty = new UserWithBookingsDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Country = user.Country ?? string.Empty,
                    Bookings = new List<BookingSummaryDto>()
                };

                return Ok(ApiResponse<UserWithBookingsDto>.SuccessResponse(empty, "No bookings"));
            }

            var dto = _mapper.Map<UserWithBookingsDto>(userWithBookings);
            return Ok(ApiResponse<UserWithBookingsDto>.SuccessResponse(dto));
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user, DateTime? expiresAt = null)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }   

            var key = _configuration["Jwt:Key"] ?? string.Empty;
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: expiresAt ?? DateTime.UtcNow.AddHours(24),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool IsValidRole(string roleName)
        {
            return roleName == Roles.Admin || roleName == Roles.User || roleName == Roles.HotelManager;
        }
    }
}

