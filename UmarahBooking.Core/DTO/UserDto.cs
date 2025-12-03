using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    public class UserDto
    {
        public int? Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password - ONLY for registration, NULL for other operations
        /// Frontend: if (isRegistration) { include password field }
        /// </summary>
        [StringLength(100, MinimumLength = 8)]
        public string? Password { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [RegularExpression("^(en|ar)$")]
        public string ?PreferredLanguage { get; set; } = "en";

        // Response-only fields
        public List<string>? Roles { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Full name helper
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }

    
}


