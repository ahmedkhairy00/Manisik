using Manisik.DTOs;
using Manisik.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class GlobalTransport
    {
        public  int TransportId { get; set; }
        public TravelMode Type { get; set; }
        public DepartureAirport DepartureAirport { get; set; }
        public ArrivalAirport ArrivalAirport { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }

        public AirlineCompany AirlineCompany { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public string FlightNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }

        public ICollection<BookingGlobalTransport> BookingGlobalTransport { get; set; } // جميع الحجوزات المرتبطة بهذه الوسيلة
    }
}
