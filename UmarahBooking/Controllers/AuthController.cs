using AutoMapper;
using Manisik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Controllers
{
    /// <summary>
    /// Controller for authentication and authorization operations
    /// Manages user registration, login, role assignment, and JWT token generation
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Dependencies

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        #endregion

        #region Role Constants

        /// <summary>
        /// Application role names
        /// </summary>
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
            public const string HotelManager = "HotelManager";
        }

        #endregion

        #region POST Operations

        /// <summary>
        /// Register a new user (automatically assigned "User" role)
        /// </summary>
        /// <param name="model">Registration details</param>
        /// <returns>JWT token and user details</returns>
        [HttpPost("Register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                        "User with this email already exists"));
                }

                // Create new user
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

                // Create user with password
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Failed to create user", errors));
                }

                // Assign "User" role to all new registrations
                await _userManager.AddToRoleAsync(user, Roles.User);

                _logger.LogInformation(
                    "New user registered successfully: {Email} with role {Role}",
                    user.Email, Roles.User);

                // Generate JWT token
                var token = await GenerateJwtToken(user);

                // Prepare response
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Country = user.Country,
                    Roles = new List<string> { Roles.User },
                    IsActive = true,
                    CreatedAt = user.CreatedAt
                };

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = userDto
                };

                return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                    authResponse,
                    "User registered successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse(
                    "An error occurred during registration"));
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token and user details</returns>
        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", model.Email);
                    return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Invalid email or password"));
                }

                // Verify password
                var result = await _signInManager.CheckPasswordSignInAsync(
                    user, model.Password, lockoutOnFailure: false);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed login attempt for user: {Email}", model.Email);
                    return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Invalid email or password"));
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                // Generate JWT token
                var token = await GenerateJwtToken(user);

                // Prepare response
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                    LastName = user.FullName.Split(' ').LastOrDefault() ?? "",
                    PhoneNumber = user.PhoneNumber,
                    Country = user.Country,
                    Roles = roles.ToList(),
                    IsActive = true,
                    CreatedAt = user.CreatedAt
                };

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = userDto
                };

                return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                    authResponse,
                    "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse(
                    "An error occurred during login"));
            }
        }

        /// <summary>
        /// Assign role to user (Admin only)
        /// </summary>
        /// <param name="model">User ID and role name</param>
        /// <returns>Success message</returns>
        [HttpPost("AssignRole")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            try
            {
                // Validate role name
                if (!IsValidRole(model.RoleName))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse(
                        $"Invalid role. Valid roles are: {string.Join(", ", GetValidRoles())}"));
                }

                // Find user
                var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        $"User with ID {model.UserId} not found"));
                }

                // Check if user already has this role
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains(model.RoleName))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse(
                        $"User already has the role '{model.RoleName}'"));
                }

                // Assign role
                var result = await _userManager.AddToRoleAsync(user, model.RoleName);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse(
                        "Failed to assign role", errors));
                }

                _logger.LogInformation(
                    "Role {Role} assigned to user {UserId} by admin",
                    model.RoleName, model.UserId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    null,
                    $"Role '{model.RoleName}' assigned successfully to user {user.Email}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    "An error occurred while assigning the role"));
            }
        }

        /// <summary>
        /// Remove role from user (Admin only)
        /// </summary>
        /// <param name="model">User ID and role name</param>
        /// <returns>Success message</returns>
        [HttpPost("RemoveRole")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto model)
        {
            try
            {
                // Find user
                var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        $"User with ID {model.UserId} not found"));
                }

                // Check if user has this role
                var hasRole = await _userManager.IsInRoleAsync(user, model.RoleName);
                if (!hasRole)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse(
                        $"User does not have the role '{model.RoleName}'"));
                }

                // Prevent removing the last Admin
                if (model.RoleName == Roles.Admin)
                {
                    var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
                    if (admins.Count == 1)
                    {
                        return BadRequest(ApiResponse<string>.ErrorResponse(
                            "Cannot remove the last Admin role from the system"));
                    }
                }

                // Remove role
                var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse(
                        "Failed to remove role", errors));
                }

                _logger.LogInformation(
                    "Role {Role} removed from user {UserId} by admin",
                    model.RoleName, model.UserId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    null,
                    $"Role '{model.RoleName}' removed successfully from user {user.Email}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing role");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    "An error occurred while removing the role"));
            }
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Get current logged-in user information
        /// </summary>
        /// <returns>User details</returns>
        [HttpGet("Me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Get user ID from claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UserDto>.ErrorResponse(
                        "Invalid token"));
                }

                // Find user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResponse(
                        "User not found"));
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                // Map to DTO
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                    LastName = user.FullName.Split(' ').LastOrDefault() ?? "",
                    PhoneNumber = user.PhoneNumber,
                    Country = user.Country,
                    Roles = roles.ToList(),
                    IsActive = true,
                    CreatedAt = user.CreatedAt
                };

                return Ok(ApiResponse<UserDto>.SuccessResponse(
                    userDto,
                    "User information retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user");
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse(
                    "An error occurred while retrieving user information"));
            }
        }

        /// <summary>
        /// Get all users with their roles (Admin only)
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet("Users")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                        LastName = user.FullName.Split(' ').LastOrDefault() ?? "",
                        PhoneNumber = user.PhoneNumber,
                        Country = user.Country,
                        Roles = roles.ToList(),
                        IsActive = true,
                        CreatedAt = user.CreatedAt
                    });
                }

                return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(
                    userDtos,
                    $"{userDtos.Count} users retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all users");
                return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResponse(
                    "An error occurred while retrieving users"));
            }
        }

        /// <summary>
        /// Get users by role (Admin only)
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <returns>List of users with the specified role</returns>
        [HttpGet("UsersByRole/{roleName}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                // Validate role
                if (!IsValidRole(roleName))
                {
                    return BadRequest(ApiResponse<IEnumerable<UserDto>>.ErrorResponse(
                        $"Invalid role. Valid roles are: {string.Join(", ", GetValidRoles())}"));
                }

                var users = await _userManager.GetUsersInRoleAsync(roleName);
                var userDtos = users.Select(user => new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                    LastName = user.FullName.Split(' ').LastOrDefault() ?? "",
                    PhoneNumber = user.PhoneNumber,
                    Country = user.Country,
                    Roles = new List<string> { roleName },
                    IsActive = true,
                    CreatedAt = user.CreatedAt
                }).ToList();

                return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(
                    userDtos,
                    $"{userDtos.Count} users with role '{roleName}' retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users by role");
                return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResponse(
                    "An error occurred while retrieving users"));
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates JWT token for authenticated user
        /// </summary>
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Get JWT settings from configuration
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates if a role name is valid
        /// </summary>
        private bool IsValidRole(string roleName)
        {
            return roleName == Roles.Admin ||
                   roleName == Roles.User ||
                   roleName == Roles.HotelManager;
        }

        /// <summary>
        /// Gets list of valid role names
        /// </summary>
        private List<string> GetValidRoles()
        {
            return new List<string> { Roles.Admin, Roles.User, Roles.HotelManager };
        }

        #endregion
    }

    #region DTOs

    /// <summary>
    /// DTO for user registration
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Country { get; set; }
    }

    /// <summary>
    /// DTO for user login
    /// </summary>
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for assigning/removing roles
    /// </summary>
    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }

    #endregion
}