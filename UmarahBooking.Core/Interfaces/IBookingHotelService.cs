using Manisik.Models;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Interfaces
{
    public interface IBookingHotelService
    {
        Task<BookingHotel> BookHotelAsync(HotelBookingDto dto, int userId);
        Task<int> CheckRoomAvailabilityAsync(HotelBookingDto dto, HotelRoom room);
        decimal CalculateTotalPrice(int numberOfRooms, int numberOfNights, decimal pricePerNight);
        int CalculateNumberOfNights(DateTime checkIn, DateTime checkOut);
        void ValidateDates(HotelBookingDto dto);
        Task<HotelRoom> GetRoomAsync(int hotelId, int roomId);
        Task<IEnumerable<PendingHotelBookingDto>> GetPendingHotelBookingsAsync(int userId);
        Task<bool> DeletePendingHotelBookingAsync(int bookingHotelId, int userId);
    }
}
