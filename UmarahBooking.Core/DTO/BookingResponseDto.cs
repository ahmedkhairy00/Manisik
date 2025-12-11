namespace UmarahBooking.Core.DTO
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int NumberOfRooms { get; set; }
        public decimal TotalPrice { get; set; }
        public string Message { get; set; }
    }
}
