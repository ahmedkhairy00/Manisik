using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    public class InternationalTransportDto
    {
        public int? Id { get; set; }

        [Required]
        public  InternationalTransportType internationalTransportType { get; set; }

        [Required]
        [StringLength(200)]
        public string CarrierName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DepartureAirport { get; set; } = string.Empty;

        [StringLength(10)]
        public string? DepartureAirportCode { get; set; }

        [Required]
        [StringLength(100)]
        public string ArrivalAirport { get; set; } = string.Empty;

        [StringLength(10)]
        public string? ArrivalAirportCode { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public DateTime ArrivalDate { get; set; }

        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 1000)]
        public int TotalSeats { get; set; }

        [Range(0, 1000)]
        public int AvailableSeats { get; set; }

        [StringLength(50)]
        public string? FlightNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
    }
}
