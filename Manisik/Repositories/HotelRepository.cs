
using Manisik.Models;
using Microsoft.EntityFrameworkCore;

namespace Manisik.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly ApplicationDbContext _context;

        public HotelRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Hotel>> GetAllHotelsAsync()
        {
            return await _context.Hotels.ToListAsync();
        }

        public async Task<Hotel?> GetHotelByIdAsync(int id)
        {
            return await _context.Hotels.FindAsync(id);
        }

        public async Task<Hotel> AddHotelAsync(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }

        public async Task<Hotel?> UpdateHotelAsync(Hotel hotel)
        {
            var existing = await _context.Hotels.FindAsync(hotel.HotelId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }

        public async Task<bool> DeleteHotelAsync(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null) return false;

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return new List<Hotel>();

            return await _context.Hotels
                .Where(h => h.City.ToLower() == city.ToLower())
                .ToListAsync();
        }
        public async Task<IEnumerable<Hotel>> GetHotelsByPriceFilterAsync(bool ascending)
        {
            if (ascending == true)
            {
                return await _context.Hotels
               // .OrderBy(h => h.PricePerNight)
                .ToListAsync();
            }
            else
            {
                return await _context.Hotels
                //.OrderByDescending(h => h.PricePerNight)
                .ToListAsync();
            }
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByRatingFilterAsync(bool ascending)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByDistanceFilterAsync(bool ascending)
        {
            if (ascending == true)
            {
                return await _context.Hotels
                .OrderBy(h => h.DistanceFromHaram)
                .ToListAsync();
            }
            else
            {
                return await _context.Hotels
                .OrderByDescending(h => h.DistanceFromHaram)
                .ToListAsync();
            }
        }

        public IQueryable<Hotel> GetAllHotelsQuerable()
        {
            return _context.Hotels.AsQueryable();
        }
    }
}
