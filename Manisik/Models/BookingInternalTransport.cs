namespace Manisik.Models
{
    public class BookingInternalTransport
    {
        public int BookingInternalTransportId { get; set; }
        public int BookingId { get; set; }
        public int InternalTransportId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string PickupLocation { get; set; }
        public string DropoffLocation { get; set; }
        public int? NumberOfPassengers { get; set; }
        public decimal TotalPrice { get; set; }

        public Booking Booking { get; set; }
        public InternalTransport internalTransport { get; set; }
    }
}
