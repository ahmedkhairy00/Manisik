using Manisik.Models;

public interface ITransportRepository
{
    Task<IEnumerable<Transport>> GetAllTransportsAsync();
    Task<Transport?> GetTransportByIdAsync(int id);
    Task<IEnumerable<Transport>> GetTransportsByTypeAsync(string vehicleType); // فلترة حسب النوع
    Task<IEnumerable<Transport>> GetTransportsByPriceAsync(decimal maxPrice); // فلترة حسب السعر
    Task<Transport> AddTransportAsync(Transport transport);
    Task<Transport?> UpdateTransportAsync(Transport transport);
    Task<bool> DeleteTransportAsync(int id);
}