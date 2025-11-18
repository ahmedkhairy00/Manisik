using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    public class TravelerDto
    {
        public int? Id { get; set; }
        public int? BookingId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z0-9]{6,9}$", ErrorMessage = "Invalid passport format")]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PassportIssuingCountry { get; set; } = string.Empty;

        [Required]
        public DateTime PassportExpiryDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        public Gender Gender { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        [Phone]
        public string? EmergencyContactPhone { get; set; }

        public bool IsMainTraveler { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}


 