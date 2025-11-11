using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class Hotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HotelId { get; set; }   // primary key

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;   // اسم الفندق

        [Required]
        public double DistanceFromHaram { get; set; }   // المسافة من الحرم

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal PricePerNight { get; set; }   // سعر الليلة

        [Required, StringLength(100)]
        public string City { get; set; } = "Makkah";   // المدينة

        public ICollection<UmrahBookingHotel>? Bookings { get; set; } // جميع الحجوزات المرتبطة بالفندق

        // Rooms in the hotel
        public ICollection<Room>? Rooms { get; set; }
    }
}
