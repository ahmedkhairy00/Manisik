using Manisik.Models;

public interface ITransportRepository
{
    Task<IEnumerable<GlobalTransport>> GetAllTransportsAsync();
    Task<GlobalTransport?> GetTransportByIdAsync(int id);
    Task<IEnumerable<GlobalTransport>> GetTransportsByTypeAsync(string vehicleType); // فلترة حسب النوع
    Task<IEnumerable<GlobalTransport>> GetTransportsByPriceAsync(decimal maxPrice); // فلترة حسب السعر
    Task<GlobalTransport> AddTransportAsync(GlobalTransport transport);
    Task<GlobalTransport?> UpdateTransportAsync(GlobalTransport transport);
    Task<bool> DeleteTransportAsync(int id);
}