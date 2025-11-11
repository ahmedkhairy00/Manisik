using Manisik.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manisik.Interfaces
{
    // ==============================
    // User / Authentication Repository
    // ==============================
    public interface IAuthRepository
    {
        Task<Auth?> GetByEmailAsync(string email);
        Task<bool> CreateUserAsync(Auth user, string password);
        Task<bool> CheckPasswordAsync(Auth user, string password);
        Task<bool> UpdateUserAsync(Auth user); // تحديث بيانات المستخدم
        Task<bool> DeleteUserAsync(int userId); // حذف المستخدم
    }
}