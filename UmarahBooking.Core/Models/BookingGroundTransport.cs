using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UmarahBooking.Core.Models
{
    public class BookingGroundTransport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingGroundTransportId { get; set; }

        [Required]
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }

        [Required]
        public int GroundTransportId { get; set; }
        [ForeignKey(nameof(GroundTransportId))]
        public GroundTransport GroundTransport{ get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; } 

        [Required]
        [MaxLength(200)]
        public string PickupLocation { get; set; } 

        [Required]
        [MaxLength(200)]
        public string DropoffLocation { get; set; }

        [Required]
        [Range(1, 100)]
        public int NumberOfPassengers { get; set; } = 1; // Default to 1 passenger

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        }
}

