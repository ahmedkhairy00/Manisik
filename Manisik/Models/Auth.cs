using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class Auth : IdentityUser<string>
    {
        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;  // الاسم الكامل للمستخدم

        [NotMapped]
        public string? DisplayRole { get; set; } = "User"; // دور المستخدم للعرض فقط (غير مخزن في قاعدة البيانات)

        public ICollection<Booking>? Booking { get; set; }  // جميع الحجوزات المرتبطة بالمستخدم
    }
}
