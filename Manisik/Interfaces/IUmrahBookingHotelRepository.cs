using Manisik.Models;
using System;
using System.Threading.Tasks;

namespace Manisik.Interfaces
{
    public interface IUmrahBookingHotelRepository
    {
        Task<BookingHotel?> AddHotelToBookingAsync(int bookingId, int hotelId, DateTime checkIn, DateTime checkOut);
        Task<BookingHotel?> UpdateHotelBookingAsync(int bookingHotelId, DateTime newCheckIn, DateTime newCheckOut);
        Task<bool> RemoveHotelFromBookingAsync(int bookingHotelId);
        Task<IEnumerable<BookingHotel>> GetHotelsByBookingIdAsync(int bookingId); // جلب جميع الفنادق لحجز معين
    }
}
