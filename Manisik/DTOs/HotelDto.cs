using System.ComponentModel.DataAnnotations;

namespace Manisik.DTOs
{
    public class HotelDto
    {
        public int HotelId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;   // اسم الفندق 

        [Required]
        public double DistanceFromHaram { get; set; }   // المسافه من الفندق للحرم 

        [Required]
        public decimal PricePerNight { get; set; }   // سعر الليلة الواحدة

        [Required, StringLength(100)]
        public string City { get; set; } = "Makkah";   // المدينة
    }
}
