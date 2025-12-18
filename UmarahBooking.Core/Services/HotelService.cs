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

            // Debug: Log total hotels before filtering
            var totalHotels = await _unitOfWork.Hotels.GetAllAsQuerable().CountAsync();
            var activeHotels = await _unitOfWork.Hotels.GetAllAsQuerable().Where(h => h.IsActive == true).CountAsync();
            Console.WriteLine($"[DEBUG] Total hotels in DB: {totalHotels}, Active hotels: {activeHotels}");
            Console.WriteLine($"[DEBUG] City filter: '{city}', Filter: '{filter}'");

            // Parse city string to enum for proper SQL translation
            if (!string.IsNullOrEmpty(city) && city.ToLower() != "all")
            {
                if (Enum.TryParse<HotelCity>(city, ignoreCase: true, out var hotelCity))
                {
                    Console.WriteLine($"[DEBUG] Filtering by city enum: {hotelCity}");
                    query = query.Where(h => h.HotelCity == hotelCity);
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Failed to parse city: '{city}'");
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
            Console.WriteLine($"[DEBUG] Hotels found after filtering: {hotels.Count}");
            
            // Debug: Print hotel names and cities
            foreach (var h in hotels)
            {
                Console.WriteLine($"[DEBUG] Hotel: {h.Name}, City: {h.HotelCity}");
            }

            if (!hotels.Any())
                return Enumerable.Empty<HotelDto>();

            try
            {
                var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);
                Console.WriteLine($"[DEBUG] Mapping successful! Mapped {hotelDtos.Count()} hotels");
                return hotelDtos;
            }
            catch (Exception mapEx)
            {
                Console.WriteLine($"[ERROR] Mapping failed: {mapEx.Message}");
                Console.WriteLine($"[ERROR] Inner exception: {mapEx.InnerException?.Message}");
                throw;
            }
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

