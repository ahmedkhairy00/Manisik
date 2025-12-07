using System.ComponentModel.DataAnnotations;

namespace UmarahBooking.Core.DTO
{
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

        public bool RememberMe { get; set; } = false;
    }
}
