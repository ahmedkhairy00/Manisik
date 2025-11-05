using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Manisik.DTOs
{
    public class UmrahBookingDto
    {
        public int UmrahBookingId { get; set; }

        [Required, StringLength(50)]
        public string TripType { get; set; } = string.Empty;  // نوع الرحله

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;   // اسم المستخدم

        [Required, StringLength(20)]
        public string NationalId { get; set; } = string.Empty; // الرقم القومي

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;   // الايميل

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;   // رقم الهاتف

        [Required, StringLength(50)]
        public string TravelMode { get; set; } = string.Empty;   // طريقة السفر

        [Required, StringLength(150)]
        public string DepartureAirport { get; set; } = string.Empty; // مطار المغادرة

        [Required, StringLength(150)]
        public string Airline { get; set; } = string.Empty;  // شركة الطيران

        [StringLength(150)]
        public string? ShipName { get; set; }  // اسم السفينة إذا كان السفر بالسفينة

        [Required]
        public DateTime StartDate { get; set; }    // تاريخ بدء السفر

        [Required]
        public DateTime EndDate { get; set; }  // تاريخ انتهاء السفر

        [Required]
        public DateTime DepartureDate { get; set; }  // تاريخ ووقت المغادرة

        [Required]
        public DateTime ArrivalDate { get; set; }   // تاريخ ووقت الوصول

        [Required]
        public decimal TravelPrice { get; set; }   // سعر الرحلة

        [Required]
        public int AuthId { get; set; }  // معرف المستخدم

        public int? TransportId { get; set; }  // معرف وسيلة النقل (اختياري)

        // قائمة الفنادق المرتبطة بالحجز
        public ICollection<UmrahBookingHotelDto>? BookingHotels { get; set; }
    }
}
