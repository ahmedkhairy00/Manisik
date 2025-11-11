using System;
using System.ComponentModel.DataAnnotations;

namespace Manisik.DTOs
{
    public class UmrahBookingHotelDto
    {
        [Required]
        public int HotelId { get; set; }  // معرف الفندق

        [Required]
        public DateTime CheckIn { get; set; }   // تاريخ الدخول

        [Required]
        public DateTime CheckOut { get; set; }  // تاريخ الخروج
    }
}
