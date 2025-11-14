using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class BookingHotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingHotelId { get; set; }  // primary key

        public int BookingId { get; set; }   // معرف الحجز
        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }  // العلاقة مع الحجز

        public decimal TotalPrice { get; set; }   // معرف المستخدم
        public int HotelId { get; set; }  // معرف الفندق
        [ForeignKey(nameof(HotelId))]
        public Hotel? Hotel { get; set; }   // العلاقة مع الفندق

        [Required]
        public DateTime CheckIn { get; set; }   // تاريخ الدخول

        [Required]
        public DateTime CheckOut { get; set; }  // تاريخ الخروج
    }
}
