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

        [Required]
        public bool? IsActive { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; } = "Makkah";   // المدينة
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? Rate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedByUserId { get; set; }



        public ICollection<BookingHotel>? Bookings { get; set; } // جميع الحجوزات المرتبطة بالفندق

       
        public ICollection<HotelRoom>? Rooms { get; set; }   // Rooms in the hotel

    }
}
