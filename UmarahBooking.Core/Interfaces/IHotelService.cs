using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Interfaces
{
    public interface IHotelService
    {
        Task<IEnumerable<HotelDto>?> GetFilteredHotelsAsync(
            string? city = null,
            string? filter = null);
        Task<HotelDto?> GetHotelByIdAsync(int id);
    }
}
