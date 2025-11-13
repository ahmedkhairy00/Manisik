using Manisik.Models;

public interface IHotelRepository
{
    Task<IEnumerable<Hotel>> GetAllHotelsAsync();
    IQueryable<Hotel> GetAllHotelsQuerable();
    Task<Hotel?> GetHotelByIdAsync(int id);
    Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city); // فلترة حسب المدينة
    Task<Hotel> AddHotelAsync(Hotel hotel);
    Task<Hotel?> UpdateHotelAsync(Hotel hotel);
    Task<bool> DeleteHotelAsync(int id);
    Task<IEnumerable<Hotel>> GetHotelsByPriceFilterAsync(bool ascending);
    Task<IEnumerable<Hotel>> GetHotelsByRatingFilterAsync(bool ascending);
    Task<IEnumerable<Hotel>> GetHotelsByDistanceFilterAsync(bool ascending);
}