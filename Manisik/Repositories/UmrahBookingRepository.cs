using Manasik.Infrastructure.Data;
using Manisik.Interfaces;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manisik.Repositories
{
    public class UmrahBookingRepository : IUmrahBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public UmrahBookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================================
        // إدارة الحجوزات الأساسية
        // ========================================
        public async Task<IEnumerable<UmrahBooking>> GetAllBookingsAsync()
        {
            return await _context.UmrahBookings
                .Include(b => b.Auth)
                .Include(b => b.Transport)
                .Include(b => b.BookingHotels)
                    .ThenInclude(bh => bh.Hotel)
                .ToListAsync();
        }

        public async Task<UmrahBooking?> GetBookingByIdAsync(int id)
        {
            return await _context.UmrahBookings
                .Include(b => b.Transport)
                .Include(b => b.BookingHotels)
                    .ThenInclude(bh => bh.Hotel)
                .FirstOrDefaultAsync(b => b.UmrahBookingId == id);
        }

        public async Task<UmrahBooking> AddBookingAsync(UmrahBooking booking)
        {
            _context.UmrahBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<UmrahBooking?> UpdateBookingAsync(UmrahBooking booking)
        {
            var existing = await _context.UmrahBookings
                .Include(b => b.BookingHotels)
                .FirstOrDefaultAsync(b => b.UmrahBookingId == booking.UmrahBookingId);

            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _context.UmrahBookings
                .Include(b => b.BookingHotels)
                .FirstOrDefaultAsync(b => b.UmrahBookingId == id);

            if (booking == null) return false;

            _context.UmrahBookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        // ========================================
        // إدارة BookingHotels
        // ========================================

        public async Task<UmrahBookingHotel?> AddHotelToBookingAsync(int bookingId, int hotelId,
                                                                    DateTime checkIn,
                                                                    DateTime checkOut)
        {
            var booking = await _context.UmrahBookings
                .Include(b => b.BookingHotels)
                .FirstOrDefaultAsync(b => b.UmrahBookingId == bookingId);

            if (booking == null) return null;

            var hotelBooking = new UmrahBookingHotel
            {
                UmrahBookingId = bookingId,
                HotelId = hotelId,
                CheckIn = checkIn,
                CheckOut = checkOut
            };

            _context.UmrahBookingHotels.Add(hotelBooking);
            await _context.SaveChangesAsync();
            return hotelBooking;
        }

        public async Task<UmrahBookingHotel?> UpdateHotelBookingAsync(int bookingHotelId,
                                                                      DateTime newCheckIn,
                                                                      DateTime newCheckOut)
        {
            var hotelBooking = await _context.UmrahBookingHotels.FindAsync(bookingHotelId);
            if (hotelBooking == null) return null;

            hotelBooking.CheckIn = newCheckIn;
            hotelBooking.CheckOut = newCheckOut;

            await _context.SaveChangesAsync();
            return hotelBooking;
        }

        public async Task<bool> RemoveHotelFromBookingAsync(int bookingHotelId)
        {
            var hotelBooking = await _context.UmrahBookingHotels.FindAsync(bookingHotelId);
            if (hotelBooking == null) return false;

            _context.UmrahBookingHotels.Remove(hotelBooking);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
