using Manisik.Models;
using System;
using System.Threading.Tasks;

namespace Manisik.Interfaces
{
    public interface IUmrahBookingHotelRepository
    {
        Task<UmrahBookingHotel?> AddHotelToBookingAsync(int bookingId, int hotelId, DateTime checkIn, DateTime checkOut);
        Task<UmrahBookingHotel?> UpdateHotelBookingAsync(int bookingHotelId, DateTime newCheckIn, DateTime newCheckOut);
        Task<bool> RemoveHotelFromBookingAsync(int bookingHotelId);
    }
}
