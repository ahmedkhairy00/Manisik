using System.ComponentModel.DataAnnotations;

namespace Manisik.DTOs
{
    // DTO لتسجيل مستخدم جديد
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;  // ايميل المستخدم

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;  // كلمة المرور
    }

    // DTO لتسجيل الدخول
    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;  // ايميل المستخدم

        [Required]
        public string Password { get; set; } = string.Empty;  // كلمة المرور
    }

    // DTO لإرجاع استجابة تسجيل الدخول / التسجيل الناجح
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;  // JWT Token

        public string Email { get; set; } = string.Empty;  // ايميل المستخدم

        public string Role { get; set; } = string.Empty; // دور المستخدم

    }
}
