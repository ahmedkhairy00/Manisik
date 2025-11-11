using Manisik.Models;

public interface IUmrahBookingRepository
{
    Task<IEnumerable<UmrahBooking>> GetAllBookingsAsync();
    Task<UmrahBooking?> GetBookingByIdAsync(int id);
    Task<IEnumerable<UmrahBooking>> GetBookingsByUserIdAsync(int userId); // فلترة حسب المستخدم
    Task<IEnumerable<UmrahBooking>> GetBookingsByDateAsync(DateTime date); // فلترة حسب التاريخ
    Task<IEnumerable<UmrahBooking>> GetBookingsByTripTypeAsync(string tripType); // فلترة حسب نوع الرحلة
    Task<UmrahBooking> AddBookingAsync(UmrahBooking booking);
    Task<UmrahBooking?> UpdateBookingAsync(UmrahBooking booking);
    Task<bool> DeleteBookingAsync(int id);

    // Manage hotel bookings associated with an UmrahBooking
    Task<UmrahBookingHotel?> AddHotelToBookingAsync(int bookingId, int hotelId, DateTime checkIn, DateTime checkOut);
    Task<UmrahBookingHotel?> UpdateHotelBookingAsync(int bookingHotelId, DateTime newCheckIn, DateTime newCheckOut);
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
    Task<UmrahBooking?> GetBookingByProviderPaymentIdAsync(string provider, string providerPaymentId);
}