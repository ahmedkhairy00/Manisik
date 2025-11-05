using Manisik.Models;

namespace Manisik.Interfaces
{
    public interface ITransportRepository
    {
        Task<IEnumerable<Transport>> GetAllTransportsAsync();
        Task<Transport?> GetTransportByIdAsync(int id);
        Task<Transport> AddTransportAsync(Transport transport);
        Task<Transport?> UpdateTransportAsync(Transport transport);
        Task<bool> DeleteTransportAsync(int id);
    }
}
