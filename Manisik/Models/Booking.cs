using Manisik.Enums;
using Manisik.Models;
//using Stripe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public string UserId { get; set; }
        public int? PackageId { get; set; }
        public TravelMode travel { get; set; }
        public string  BookingNumber { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }
        public int? CurrentStep { get; set; } = 1;

        // Payment Info
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentIntentId { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Cancellation
        public DateTime CancellationDate { get; set; }
        public string? CancellationReason { get; set; }
        public decimal? RefundAmount { get; set; }

        

        // Navigation
        public Auth User { get; set; }
        public ICollection<BookingHotel> BookingHotel { get; set; }
        public ICollection<BookingInternalTransport> BookingInternalTransport { get; set; }
        public ICollection<BookingGlobalTransport> BookingGlobalTransport { get; set; }
        public ICollection<Traveler> Travelers { get; set; }
        public ICollection<PaymentEvent> PaymentEvents { get; set; }
    }
}