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
        public string? PreferredLanguage { get; set; } = "en";

        // Response-only fields
        public List<string>? Roles { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Full name helper
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }

    public class UserWithBookingsDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public List<BookingSummaryDto> Bookings { get; set; }
    }

    public class BookingSummaryDto
    {
        public int BookingId { get; set; }
        public string BookingNumber { get; set; }
        public string BookingType { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public int HotelsCount { get; set; }
        public int TravelersCount { get; set; }
    }

}


