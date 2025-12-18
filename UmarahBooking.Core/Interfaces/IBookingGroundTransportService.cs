using UmarahBooking.Core.Models;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Interfaces
{
    /// <summary>
    /// Service interface for ground transport booking operations
    /// </summary>
    public interface IBookingGroundTransportService
    {
        /// <summary>
        /// Book ground transport for a user
        /// </summary>
        /// <param name="dto">Ground transport booking details</param>
        /// <param name="userId">ID of the user making the booking</param>
        /// <returns>Created BookingGroundTransport entity</returns>
        Task<BookingGroundTransport> BookGroundTransportAsync(GroundTransportBookingDto dto, int userId);

        /// <summary>
        /// Validate service date is in the future
        /// </summary>
        /// <param name="serviceDate">Service date to validate</param>
        void ValidateServiceDate(DateTime serviceDate);

        /// <summary>
        /// Calculate total price for ground transport
        /// </summary>
        /// <param name="passengers">Number of passengers</param>
        /// <param name="pricePerPerson">Price per person</param>
        /// <returns>Total price</returns>
        decimal CalculateTotalPrice(int passengers, decimal pricePerPerson);

        /// <summary>
        /// Get ground transport by ID
        /// </summary>
        /// <param name="transportId">Ground transport ID</param>
        /// <returns>Ground transport entity</returns>
        Task<GroundTransport> GetGroundTransportAsync(int transportId);
        Task<IEnumerable<PendingGroundBookingDto>> GetPendingGroundBookingsAsync(int userId);
        Task<bool> DeletePendingGroundBookingAsync(int bookingGroundTransportId, int userId);
    }
}

