namespace Manisik.Models
{
    public class BookingGlobalTransport
    {
        public int BookingGlobalTransportid { get; set; }
        public int BookingId { get; set; }
        public int TransportId { get; set; }
        
        public decimal Price { get; set; }

        public Booking Booking { get; set; }
        public GlobalTransport Transport { get; set; }
    }
}
