using UmarahBooking.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UmarahBooking.Core.Models
{
    public class InternationalTransport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InternationalTransportId { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]

        public InternationalTransportType TransportType { get; set; }

        [MaxLength(100)]
        public string CarrierName { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]

        public DepartureAirport DepartureAirport { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]

        public ArrivalAirport ArrivalAirport { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public DateTime ArrivalDate { get; set; }

        public DateTime? ReturnDate { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string Duration { get; set; } = string.Empty; // NEW

        [Required]
        [Range(1, 500)]
        public int AvailableSeats { get; set; }

        [MaxLength(20)]
        public string FlightNumber { get; set; }

        [Required]
        [Range(1, 500)]
        public int TotalSeats { get; set; } = 52; // NEW: total seats of the transport

        [Required]
        public bool IsActive { get; set; }

        //public int Duration { get; set; }


        [MaxLength(50)]
        public flightDegree FlightClass { get; set; }

        public int? rate { get; set; }

        public int? review { get; set; }

        [MaxLength(50)]
        public string Stops { get; set; }

        // Navigation
        public ICollection<BookingInternationalTransport> BookingInternationalTransport { get; set; }
    }
}

