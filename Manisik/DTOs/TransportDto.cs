using System.ComponentModel.DataAnnotations;

namespace Manisik.DTOs
{
    public class TransportDto
    {
        [Required, StringLength(100)]
        public string VehicleType { get; set; } = string.Empty;   // نوع المواصلات

        [Required]
        public decimal Price { get; set; }  // سعر المواصلات

        [Required, StringLength(100)]
        public string ProviderName { get; set; } = string.Empty;  // اسم الشركة
    }
}
