using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class Transport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransportId { get; set; }  // primary key

        [Required, StringLength(100)]
        public string VehicleType { get; set; } = string.Empty;   // نوع وسيلة النقل (طيارة/سفينة/باص)

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }  // سعر النقل

        [Required, StringLength(100)]
        public string ProviderName { get; set; } = string.Empty;  // اسم الشركة

        public ICollection<UmrahBooking>? Bookings { get; set; } // جميع الحجوزات المرتبطة بهذه الوسيلة
    }
}
