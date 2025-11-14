using Manasik.Infrastructure.Data;
using Manisik.Interfaces;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.UmrahBookings
                .Include(b => b.Auth)
                .Include(b => b.Transport)
                .Include(b => b.BookingHotels)
                    .ThenInclude(bh => bh.Hotel)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.UmrahBookings
                .Include(b => b.Transport)
                .Include(b => b.BookingHotels)
                    .ThenInclude(bh => bh.Hotel)
                .FirstOrDefaultAsync(b => b.UmrahBookingId == id);
        }

        public async Task<Booking> AddBookingAsync(Booking booking)
        {
            _context.UmrahBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> UpdateBookingAsync(Booking booking)
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
        public async Task<BookingHotel?> AddHotelToBookingAsync(int bookingId, int hotelId,
                                                                    DateTime checkIn,
                                                                    DateTime checkOut)
        {
            var booking = await _context.UmrahBookings
                .Include(b => b.BookingHotels)
                .FirstOrDefaultAsync(b => b.UmrahBookingId == bookingId);

            if (booking == null) return null;

            var hotelBooking = new BookingHotel
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

        public async Task<BookingHotel?> UpdateHotelBookingAsync(int bookingHotelId,
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

        // ========================================
        // فلترة الحجوزات
        // ========================================
        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _context.UmrahBookings
                .Where(b => b.AuthId == userId)
                .Include(b => b.BookingHotels)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateTime date)
        {
            return await _context.UmrahBookings
                .Where(b => b.StartDate.Date <= date.Date && b.EndDate.Date >= date.Date)
                .Include(b => b.BookingHotels)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByTripTypeAsync(string tripType)
        {
            return await _context.UmrahBookings
                .Where(b => b.TripType.ToLower() == tripType.ToLower())
                .Include(b => b.BookingHotels)
                .ToListAsync();
        }

        // ========================================
        // إدارة المدفوعات
        // ========================================
        public async Task<bool> TryMarkBookingPaidByProviderEventAsync(string provider, string providerPaymentId, string providerEventId, DateTime? capturedAt)
        {
            var booking = await _context.UmrahBookings
                .FirstOrDefaultAsync(b => b.PaymentProvider == provider && b.PaymentProviderId == providerPaymentId);

            if (booking == null)
                return false;

            booking.IsPaid = true;
            booking.PaymentCapturedAt = capturedAt ?? DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TryMarkBookingRefundedAsync(string provider, string providerPaymentId, DateTime? refundedAt)
        {
            var booking = await _context.UmrahBookings
                .FirstOrDefaultAsync(b => b.PaymentProvider == provider && b.PaymentProviderId == providerPaymentId);

            if (booking == null || !booking.IsPaid)
                return false;

            booking.IsPaid = false;
            booking.PaymentCapturedAt = refundedAt ?? DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Booking?> GetBookingByProviderPaymentIdAsync(string provider, string providerPaymentId)
        {
            return await _context.UmrahBookings
                .FirstOrDefaultAsync(b => b.PaymentProvider == provider && b.PaymentProviderId == providerPaymentId);
        }
    }
}
