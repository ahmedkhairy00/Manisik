using Manisik.Models;

namespace Manisik.Interfaces
{
    public interface IHotelRepository
    {
        Task<IEnumerable<Hotel>> GetAllHotelsAsync();
        Task<Hotel?> GetHotelByIdAsync(int id);
        Task<Hotel> AddHotelAsync(Hotel hotel);
        Task<Hotel?> UpdateHotelAsync(Hotel hotel);
        Task<bool> DeleteHotelAsync(int id);
    }
}
