using System.ComponentModel.DataAnnotations;

namespace UmarahBooking.Core.DTO
{
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
}
