using System;
using System.ComponentModel.DataAnnotations;

namespace Manisik.DTOs
{
    public class ReserveHotelDto
    {
        // booking info
        [Required]
        public string TripType { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string NationalId { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        [Required]
        public string TravelMode { get; set; } = string.Empty;
        [Required]
        public string DepartureAirport { get; set; } = string.Empty;
        [Required]
        public string Airline { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public DateTime DepartureDate { get; set; }
        [Required]
        public DateTime ArrivalDate { get; set; }
        [Required]
        public decimal TravelPrice { get; set; }
        [Required]
        public int AuthId { get; set; }

        // hotel-specific
        [Required]
        public int HotelId { get; set; }
        [Required]
        public DateTime CheckIn { get; set; }
        [Required]
        public DateTime CheckOut { get; set; }
    }

    public class ReserveTransportDto
    {
        [Required]
        public string TripType { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string NationalId { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        [Required]
        public string TravelMode { get; set; } = string.Empty;
        [Required]
        public string DepartureAirport { get; set; } = string.Empty;
        [Required]
        public string Airline { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public DateTime DepartureDate { get; set; }
        [Required]
        public DateTime ArrivalDate { get; set; }
        [Required]
        public decimal TravelPrice { get; set; }
        [Required]
        public int AuthId { get; set; }

        [Required]
        public int TransportId { get; set; }
    }
}
