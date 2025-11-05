using AutoMapper;
using Manisik.DTOs;
using Manisik.Interfaces;
using Manisik.Models;

namespace Manisik.Services
{
    public class TransportService
    {
        private readonly ITransportRepository _transportRepo;
        private readonly IMapper _mapper;

        public TransportService(ITransportRepository transportRepo, IMapper mapper)
        {
            _transportRepo = transportRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransportDto>> GetAllAsync()
        {
            var transports = await _transportRepo.GetAllTransportsAsync();
            return _mapper.Map<IEnumerable<TransportDto>>(transports);
        }

        public async Task<TransportDto?> GetByIdAsync(int id)
        {
            var transport = await _transportRepo.GetTransportByIdAsync(id);
            return _mapper.Map<TransportDto?>(transport);
        }

        public async Task<TransportDto> AddAsync(TransportDto dto)
        {
            var entity = _mapper.Map<Transport>(dto);
            var result = await _transportRepo.AddTransportAsync(entity);
            return _mapper.Map<TransportDto>(result);
        }

        public async Task<TransportDto?> UpdateAsync(int id, TransportDto dto)
        {
            var existing = await _transportRepo.GetTransportByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            var updated = await _transportRepo.UpdateTransportAsync(existing);
            return _mapper.Map<TransportDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _transportRepo.DeleteTransportAsync(id);
        }
    }
}
