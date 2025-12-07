using System;

namespace UmarahBooking.Core.DTO
{
    public class PendingTransportBookingDto
    {
        public int BookingId { get; set; }
        public int BookingInternationalTransportId { get; set; }
        public int InternationalTransportId { get; set; } // Added for compatibility
        public int TransportId { get; set; }
        public string TransportType { get; set; }
        public string CarrierName { get; set; }
        public string FlightNumber { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public int NumberOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
    }
}