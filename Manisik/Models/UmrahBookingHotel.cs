using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class UmrahBookingHotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UmrahBookingHotelId { get; set; }  // primary key

        public int UmrahBookingId { get; set; }   // معرف الحجز
        [ForeignKey(nameof(UmrahBookingId))]
        public UmrahBooking? UmrahBooking { get; set; }  // العلاقة مع الحجز

        public int HotelId { get; set; }  // معرف الفندق
        [ForeignKey(nameof(HotelId))]
        public Hotel? Hotel { get; set; }   // العلاقة مع الفندق

        [Required]
        public DateTime CheckIn { get; set; }   // تاريخ الدخول

        [Required]
        public DateTime CheckOut { get; set; }  // تاريخ الخروج
    }
}
