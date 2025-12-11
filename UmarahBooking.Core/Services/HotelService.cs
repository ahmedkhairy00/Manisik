using AutoMapper;
using UmarahBooking.Core.Enums;
using UmarahBooking.Core.Models;
using Microsoft.EntityFrameworkCore;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    public class HotelService : IHotelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public HotelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HotelDto>?> GetFilteredHotelsAsync(
            string? city = null,
            string? filter = null)
        {
            IQueryable<Hotel> query = _unitOfWork.Hotels.GetAllAsQuerable()
                .Include(h => h.Rooms)
                .Where(h => h.IsActive == true);

            // Parse city string to enum for proper SQL translation
            if (!string.IsNullOrEmpty(city) && city.ToLower() != "all")
            {
                if (Enum.TryParse<HotelCity>(city, ignoreCase: true, out var hotelCity))
                {
                    query = query.Where(h => h.HotelCity == hotelCity);
                }
            }

            if (!string.IsNullOrEmpty(filter))
            {
                query = filter.ToLower() switch
                {
                    "distance" => query.OrderBy(h => h.DistanceToHaram),
                    "rating" => query.OrderByDescending(h => h.StarRating),
                    _ => query
                };
            }

            var hotels = await query.ToListAsync();

            if (!hotels.Any())
                return Enumerable.Empty<HotelDto>();

            var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);
            return hotelDtos;
        }

        public async Task<HotelDto?> GetHotelByIdAsync(int id)
        {
            var hotel = await _unitOfWork.Hotels.GetAllAsQuerable()
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                return null;

            //hotel.Rooms = hotel.Rooms?.Where(r => r.IsActive).ToList();
            hotel.Rooms = hotel.Rooms?.ToList();

            var hotelDto = _mapper.Map<HotelDto>(hotel);
            return hotelDto;
        }


    }
}

