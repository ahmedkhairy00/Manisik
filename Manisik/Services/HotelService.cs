using AutoMapper;
using Manisik.DTOs;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;

namespace Manisik.Services
{
    public class HotelService
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IMapper _mapper;

        public HotelService(IHotelRepository hotelRepo, IMapper mapper)
        {
            _hotelRepo = hotelRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HotelDto>> GetAllAsync()
        {
            var hotels = await _hotelRepo.GetAllHotelsAsync();
            return _mapper.Map<IEnumerable<HotelDto>>(hotels);
        }

        public async Task<HotelDto?> GetByIdAsync(int id)
        {
            var hotel = await _hotelRepo.GetHotelByIdAsync(id);
            return _mapper.Map<HotelDto?>(hotel);
        }

        public async Task<HotelDto> AddAsync(HotelDto dto)
        {
            var entity = _mapper.Map<Hotel>(dto);
            var result = await _hotelRepo.AddHotelAsync(entity);
            return _mapper.Map<HotelDto>(result);
        }

        public async Task<HotelDto?> UpdateAsync(int id, HotelDto dto)
        {
            var existing = await _hotelRepo.GetHotelByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            var updated = await _hotelRepo.UpdateHotelAsync(existing);
            return _mapper.Map<HotelDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _hotelRepo.DeleteHotelAsync(id);
        }

        public async Task<IEnumerable<HotelDto>?> GetHotelsByCityAsync(string city)
        {
            var hotels = await _hotelRepo.GetHotelsByCityAsync(city);
            if (hotels == null)
                return null;
            return hotels.Select(h => _mapper.Map<HotelDto>(h));
        }
        public async Task<IEnumerable<HotelDto>?> GetHotelsByPriceFilterAsync(bool ascending)
        {
            var hotels = await _hotelRepo.GetHotelsByPriceFilterAsync(ascending);
            if (hotels == null)
                return null;
            return hotels.Select(h => _mapper.Map<HotelDto>(h));
        }

        public async Task<IEnumerable<HotelDto>?> GetHotelsByDistanceFilterAsync(bool ascending)
        {
            var hotels = await _hotelRepo.GetHotelsByDistanceFilterAsync(ascending);
            if (hotels == null)
                return null;
            return hotels.Select(h => _mapper.Map<HotelDto>(h));
        }

        public async Task<IEnumerable<HotelDto>?> GetFilteredHotelsAsync(
            string? city = null,
            string? filter = null)
        {
            IQueryable<Hotel> query = _hotelRepo.GetAllHotelsQuerable();

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(h => h.City.ToLower() == city.ToLower());


            }

            // Apply sorting based on filter
            if (!string.IsNullOrEmpty(filter))
            {
                query = filter.ToLower() switch
                {
                    "pricelowtohigh" => query.OrderBy(h => h.PricePerNight),
                    "pricehightolow" => query.OrderByDescending(h => h.PricePerNight),
                    "distance" => query.OrderBy(h => h.DistanceFromHaram),
                    // "rating" => query.OrderByDescending(h => h.rating),
                    _ => query
                };
            }

            var hotels = await query.ToListAsync();

            if (!hotels.Any())
                return null;

            return hotels.Select(h => _mapper.Map<HotelDto>(h));
        }

    }
}
