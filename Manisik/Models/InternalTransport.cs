namespace Manisik.Models
{
    public class InternalTransport
    {
        public int internalTransportId { get; set; }
        public string CompanyName { get; set; }
        public InternalTransport Type { get; set; }
        public decimal Price { get; set; }
        public bool? IsActive { get; set; }

        public int? rating { get; set; } 
        public DateTime? CreatedAt { get; set; }

        public ICollection<BookingInternalTransport> BookingGroundTransports { get; set; }
    }
}
