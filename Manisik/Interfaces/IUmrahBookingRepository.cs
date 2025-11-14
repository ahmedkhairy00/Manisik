using Manisik.Models;

public interface IUmrahBookingRepository
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId); // فلترة حسب المستخدم
    Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateTime date); // فلترة حسب التاريخ
    Task<IEnumerable<Booking>> GetBookingsByTripTypeAsync(string tripType); // فلترة حسب نوع الرحلة
    Task<Booking> AddBookingAsync(Booking booking);
    Task<Booking?> UpdateBookingAsync(Booking booking);
    Task<bool> DeleteBookingAsync(int id);

    // Manage hotel bookings associated with an UmrahBooking
    Task<BookingHotel?> AddHotelToBookingAsync(int bookingId, int hotelId, DateTime checkIn, DateTime checkOut);
    Task<BookingHotel?> UpdateHotelBookingAsync(int bookingHotelId, DateTime newCheckIn, DateTime newCheckOut);
    Task<bool> RemoveHotelFromBookingAsync(int bookingHotelId);

    // Payment processing helpers
    /// <summary>
    /// Attempts to mark the booking as paid.
    /// Ensures idempotency using providerEventId.
    /// </summary>
    Task<bool> TryMarkBookingPaidByProviderEventAsync(string provider, string providerPaymentId, string providerEventId, DateTime? capturedAt);

    /// <summary>
    /// Attempts to mark the booking as refunded.
    /// </summary>
    Task<bool> TryMarkBookingRefundedAsync(string provider, string providerPaymentId, DateTime? refundedAt);

    // Find booking by provider and provider payment id
    Task<Booking?> GetBookingByProviderPaymentIdAsync(string provider, string providerPaymentId);
}