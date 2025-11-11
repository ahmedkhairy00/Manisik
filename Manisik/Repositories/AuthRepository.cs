using Manisik.Interfaces;
using Manisik.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Manisik.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<Auth> _userManager;
        private readonly SignInManager<Auth> _signInManager;

        public AuthRepository(UserManager<Auth> userManager, SignInManager<Auth> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Auth?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> CreateUserAsync(Auth user, string password)
        {
            if (user == null)
                throw new System.ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(password))
                throw new System.ArgumentException("Password cannot be empty.", nameof(password));

            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> CheckPasswordAsync(Auth user, string password)
        {
            if (user == null || string.IsNullOrWhiteSpace(password))
                return false;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }
        public async Task<bool> UpdateUserAsync(Auth user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

    }
}
