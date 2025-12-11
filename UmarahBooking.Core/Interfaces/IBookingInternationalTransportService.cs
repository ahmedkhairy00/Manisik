using UmarahBooking.Core.Models;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Interfaces
{
    /// <summary>
    /// Service interface for international transport booking operations
    /// </summary>
    public interface IBookingInternationalTransportService
    {
        /// <summary>
        /// Book international transport for a user
        /// </summary>
        /// <param name="dto">International transport booking details</param>
        /// <param name="userId">ID of the user making the booking</param>
        /// <returns>Created BookingInternationalTransport entity</returns>
        Task<BookingInternationalTransport> BookInternationalTransportAsync(InternationalTransportBookingDto dto, int userId);

        /// <summary>
        /// Validate departure date is in the future
        /// </summary>
        /// <param name="departureDate">Departure date to validate</param>
        void ValidateDepartureDate(DateTime departureDate);

        /// <summary>
        /// Calculate total price for international transport
        /// </summary>
        /// <param name="seats">Number of seats</param>
        /// <param name="pricePerSeat">Price per seat</param>
        /// <returns>Total price</returns>
        decimal CalculateTotalPrice(int seats, decimal pricePerSeat);

        /// <summary>
        /// Get international transport by ID
        /// </summary>
        /// <param name="transportId">International transport ID</param>
        /// <returns>International transport entity</returns>
        Task<InternationalTransport> GetInternationalTransportAsync(int transportId);

        /// <summary>
        /// Check seat availability for international transport
        /// </summary>
        /// <param name="transportId">International transport ID</param>
        /// <param name="requestedSeats">Number of requested seats</param>
        /// <returns>Number of available seats</returns>
        Task<int> CheckSeatAvailabilityAsync(int transportId, int requestedSeats);
        Task<IEnumerable<PendingTransportBookingDto>> GetPendingTransportBookingsAsync(int userId);
        Task<bool> DeletePendingInternationalBookingAsync(int bookingInternationalTransportId, int userId);
    }
}

