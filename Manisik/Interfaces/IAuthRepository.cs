using Manisik.Models;
using System.Threading.Tasks;

namespace Manisik.Interfaces
{
    public interface IAuthRepository
    {
        Task<Auth?> GetByEmailAsync(string email);
        Task<bool> CreateUserAsync(Auth user, string password);
        Task<bool> CheckPasswordAsync(Auth user, string password);
    }
}
