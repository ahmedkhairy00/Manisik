using AutoMapper;
using Manisik.DTOs;
using Manisik.Interfaces;
using Manisik.Models;

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
    }
}
