using Manisik.Enums;
using System.ComponentModel.DataAnnotations;

namespace UmarahBooking.Core.DTO
{
    /// <summary>
    /// International transport booking details
    /// </summary>
    public class InternationalTransportBookingDto
    {
        public int ?TransportId { get; set; }

        public InternationalTransportType? Type { get; set; }
        public string? CarrierName { get; set; }
        public string? FlightNumber { get; set; }
        public string? ShipNumber { get; set; }

        public string? DepartureAirport { get; set; }
        public string? ArrivalAirport { get; set; }
        public DateTime? DepartureDate { get; set; }

        [Range(1, 50)]
        public int NumberOfSeats { get; set; }

        public decimal? PricePerSeat { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
