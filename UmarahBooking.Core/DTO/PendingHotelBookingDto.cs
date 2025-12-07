using System;

namespace UmarahBooking.Core.DTO
{
    public class PendingHotelBookingDto
    {
        public int BookingId { get; set; }
        public int BookingHotelId { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string City { get; set; }
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public int NumberOfRooms { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}