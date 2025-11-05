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
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> CreateUserAsync(Auth user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> CheckPasswordAsync(Auth user, string password)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }
    }
}
