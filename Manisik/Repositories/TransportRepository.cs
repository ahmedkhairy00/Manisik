using Manasik.Infrastructure.Data;
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

        public async Task<IEnumerable<Transport>> GetAllTransportsAsync()
        {
            return await _context.Transports.ToListAsync();
        }

        public async Task<Transport?> GetTransportByIdAsync(int id)
        {
            return await _context.Transports.FindAsync(id);
        }

        public async Task<Transport> AddTransportAsync(Transport transport)
        {
            _context.Transports.Add(transport);
            await _context.SaveChangesAsync();
            return transport;
        }

        public async Task<Transport?> UpdateTransportAsync(Transport transport)
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
    }
}
