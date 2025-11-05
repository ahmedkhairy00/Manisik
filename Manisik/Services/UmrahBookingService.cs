using AutoMapper;
using Manisik.DTOs;
using Manisik.Interfaces;
using Manisik.Models;

namespace Manisik.Services
{
    public class UmrahBookingService
    {
        private readonly IUmrahBookingRepository _bookingRepo;
        private readonly IMapper _mapper;

        public UmrahBookingService(IUmrahBookingRepository bookingRepo, IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UmrahBookingDto>> GetAllAsync()
        {
            var bookings = await _bookingRepo.GetAllBookingsAsync();
            return _mapper.Map<IEnumerable<UmrahBookingDto>>(bookings);
        }

        public async Task<UmrahBookingDto?> GetByIdAsync(int id)
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(id);
            return _mapper.Map<UmrahBookingDto?>(booking);
        }

        public async Task<UmrahBookingDto> AddAsync(UmrahBookingDto dto)
        {
            var entity = _mapper.Map<UmrahBooking>(dto);
            var result = await _bookingRepo.AddBookingAsync(entity);
            return _mapper.Map<UmrahBookingDto>(result);
        }

        public async Task<UmrahBookingDto?> UpdateAsync(int id, UmrahBookingDto dto)
        {
            var existing = await _bookingRepo.GetBookingByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            var updated = await _bookingRepo.UpdateBookingAsync(existing);
            return _mapper.Map<UmrahBookingDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _bookingRepo.DeleteBookingAsync(id);
        }
    }
}
