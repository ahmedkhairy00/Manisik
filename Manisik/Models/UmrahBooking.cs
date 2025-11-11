using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class UmrahBooking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UmrahBookingId { get; set; }  // primary key

        [Required, StringLength(50)]
        public string TripType { get; set; } = string.Empty;  // نوع الرحلة (عمرة/حج)

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;   // اسم المستخدم

        [Required, StringLength(20)]
        public string NationalId { get; set; } = string.Empty; // الرقم القومي

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;   // البريد الإلكتروني

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;   // رقم الهاتف

        [Required, StringLength(50)]
        public string TravelMode { get; set; } = string.Empty;   // وسيلة السفر

        [Required, StringLength(150)]
        public string DepartureAirport { get; set; } = string.Empty; // المطار المغادر منه

        [Required, StringLength(150)]
        public string Airline { get; set; } = string.Empty;  // شركة الطيران

        [StringLength(150)]
        public string? ShipName { get; set; }  // اسم السفينة إذا كانت الرحلة بحرية

        [Required]
        public DateTime StartDate { get; set; }    // بداية الرحلة

        [Required]
        public DateTime EndDate { get; set; }  // نهاية الرحلة

        [Required]
        public DateTime DepartureDate { get; set; }  // وقت المغادرة

        [Required]
        public DateTime ArrivalDate { get; set; }   // وقت الوصول

        [Column(TypeName = "decimal(10,2)")]
        public decimal TravelPrice { get; set; }   // سعر الرحلة

        // العلاقات
        public int AuthId { get; set; }  // معرف المستخدم
        [ForeignKey(nameof(AuthId))]
        public Auth? Auth { get; set; }  // العلاقة مع جدول المستخدمين

        public int? TransportId { get; set; }  // معرف وسيلة النقل
        [ForeignKey(nameof(TransportId))]
        public Transport? Transport { get; set; }  // العلاقة مع وسائل النقل

        public ICollection<UmrahBookingHotel>? BookingHotels { get; set; } // الفنادق المرتبطة بالحجز

        // Payment metadata
        public string? PaymentProvider { get; set; } // e.g., "Stripe" or "PayPal"
        public string? PaymentProviderId { get; set; } // PaymentIntentId or PayPal OrderId
        public string? PaymentStatus { get; set; } // e.g., "succeeded", "COMPLETED"
        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentCapturedAt { get; set; }
    }
}
