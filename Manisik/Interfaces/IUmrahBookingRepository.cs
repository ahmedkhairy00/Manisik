using Manisik.Models;

namespace Manisik.Interfaces
{
    public interface IUmrahBookingRepository
    {
        Task<IEnumerable<UmrahBooking>> GetAllBookingsAsync();
        Task<UmrahBooking?> GetBookingByIdAsync(int id);
        Task<UmrahBooking> AddBookingAsync(UmrahBooking booking);
        Task<UmrahBooking?> UpdateBookingAsync(UmrahBooking booking);
        Task<bool> DeleteBookingAsync(int id);
    }
}
