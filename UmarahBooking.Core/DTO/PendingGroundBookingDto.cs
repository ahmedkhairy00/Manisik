using System;

namespace UmarahBooking.Core.DTO
{
    public class PendingGroundBookingDto
    {
        public int BookingId { get; set; }
        public int BookingGroundTransportId { get; set; }
        public int GroundTransportId { get; set; }
        public string ServiceName { get; set; }
        public string PickupLocation { get; set; }
        public string DropoffLocation { get; set; }
        public DateTime? ServiceDate { get; set; }
        public int NumberOfPassengers { get; set; }
        public decimal TotalPrice { get; set; }
    }
}