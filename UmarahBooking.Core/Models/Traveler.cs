using Manisik.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class Traveler
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TravelerId { get; set; }

        [Required]
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(50)]
        public string PassportNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PassportExpiryDate { get; set; }  // Must PassportExpiryDate > 6 Months

        [Required]
        [MaxLength(50)]
        public string Nationality { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]

        public Gender Gender { get; set; }

        [MaxLength(20)]
        [Phone]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public bool? IsMainTraveler { get; set; } = true;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
