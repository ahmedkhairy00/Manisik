using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    public class TransportBookingDto
    {
        [Required]
        public int TransportId { get; set; }

        public InternationalTransportType? Type { get; set; }
        public string? CarrierName { get; set; }
        public string? FlightNumber { get; set; }

        public string? ShiptNumber { get; set; }

        public string? DepartureAirport { get; set; }
        public string? ArrivalAirport { get; set; }
        public DateTime? DepartureDate { get; set; }

        [Range(1, 50)]
        public int NumberOfSeats { get; set; }

        public decimal? PricePerSeat { get; set; }
        public decimal? TotalPrice { get; set; }
    }

   
}
