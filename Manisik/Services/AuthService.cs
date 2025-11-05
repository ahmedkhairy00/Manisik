using Manisik.Interfaces;
using Manisik.Models;
using Microsoft.AspNetCore.Identity;

namespace Manisik.Services
{
    public class AuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly UserManager<Auth> _userManager;

        public AuthService(IAuthRepository authRepository, UserManager<Auth> userManager)
        {
            _authRepository = authRepository;
            _userManager = userManager;
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            var user = new Auth
            {
                UserName = email,
                Email = email,
                FullName = email // temporary default; you may accept full name in DTO later
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            // ensure default role exists and assign
            var roleExists = await _userManager.IsInRoleAsync(user, "User");
            if (!roleExists)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            return true;
        }

        public async Task<Auth?> LoginAsync(string email, string password)
        {
            var user = await _authRepository.GetByEmailAsync(email);
            if (user == null) return null;

            var valid = await _authRepository.CheckPasswordAsync(user, password);
            return valid ? user : null;
        }
    }
}
