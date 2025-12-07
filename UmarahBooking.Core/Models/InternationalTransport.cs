using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
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

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 500)]
        public int AvailableSeats { get; set; }

        [MaxLength(20)]
        public string FlightNumber { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // Navigation
        public ICollection<BookingInternationalTransport> BookingInternationalTransport { get; set; }
    }
}
