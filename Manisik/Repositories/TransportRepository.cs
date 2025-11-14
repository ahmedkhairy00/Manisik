
using Manisik.Interfaces;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manisik.Repositories
{
    public class TransportRepository : ITransportRepository
    {
        private readonly ApplicationDbContext _context;

        public TransportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GlobalTransport>> GetAllTransportsAsync()
        {
            return await _context.Transports.ToListAsync();
        }

        public async Task<GlobalTransport?> GetTransportByIdAsync(int id)
        {
            return await _context.Transports.FindAsync(id);
        }

        public async Task<GlobalTransport> AddTransportAsync(GlobalTransport transport)
        {
            _context.Transports.Add(transport);
            await _context.SaveChangesAsync();
            return transport;
        }

        public async Task<GlobalTransport?> UpdateTransportAsync(GlobalTransport transport)
        {
            var existing = await _context.Transports.FindAsync(transport.TransportId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(transport);
            await _context.SaveChangesAsync();
            return transport;
        }

        public async Task<bool> DeleteTransportAsync(int id)
        {
            var transport = await _context.Transports.FindAsync(id);
            if (transport == null) return false;

            _context.Transports.Remove(transport);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<GlobalTransport>> GetTransportsByTypeAsync(string vehicleType)
        {
            if (string.IsNullOrWhiteSpace(vehicleType))
                return new List<GlobalTransport>();

            return await _context.Transports
                .Where(t => t.VehicleType.ToLower() == vehicleType.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<GlobalTransport>> GetTransportsByPriceAsync(decimal maxPrice)
        {
            return await _context.Transports
                .Where(t => t.Price <= maxPrice)
                .ToListAsync();
        }

    }
}
