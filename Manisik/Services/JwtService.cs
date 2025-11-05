using Manisik.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Manisik.Services
{
    /// <summary>
    /// خدمة لإنشاء JSON Web Token (JWT) للمستخدمين
    /// الآن يستخدم UserManager لاستخراج أدوار المستخدم وتضمينها في التوكن
    /// </summary>
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string? _issuer;
        private readonly string? _audience;
        private readonly UserManager<Auth> _userManager;

        public JwtService(IConfiguration configuration, UserManager<Auth> userManager)
        {
            _userManager = userManager;
            _secretKey = configuration["Jwt:Key"] ?? string.Empty;
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }

        /// <summary>
        /// توليد توكن JWT لمستخدم معين (يضم أدوار المستخدم)
        /// </summary>
        /// <param name="user">المستخدم</param>
        /// <returns>توكين JWT على شكل نص</returns>
        public async Task<string> GenerateTokenAsync(Auth user)
        {
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            // add role claims from UserManager
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _issuer,
                Audience = _audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<IList<string>> GetRolesForUserAsync(Auth user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}
