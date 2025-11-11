using Manisik.Interfaces;
using Manisik.Models;
using System.Threading.Tasks;

namespace Manisik.Services
{
    public class AuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new System.ArgumentException("Email cannot be empty.");
            if (string.IsNullOrWhiteSpace(password))
                throw new System.ArgumentException("Password cannot be empty.");

            var existingUser = await _authRepository.GetByEmailAsync(email);
            if (existingUser != null)
                throw new System.InvalidOperationException("User already exists.");

            var user = new Auth
            {
                UserName = email,
                Email = email,
                // Role = "User"
            };

            return await _authRepository.CreateUserAsync(user, password);
        }

        public async Task<Auth?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _authRepository.GetByEmailAsync(email);
            if (user == null) return null;

            var valid = await _authRepository.CheckPasswordAsync(user, password);
            return valid ? user : null;
        }
    }
}
