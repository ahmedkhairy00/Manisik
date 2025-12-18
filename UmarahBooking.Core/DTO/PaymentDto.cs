using UmarahBooking.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    public class PaymentDto
    {
        public int? Id { get; set; }
        public int? BookingId { get; set; }

        [Range(0.01, 1000000)]
        public decimal? Amount { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;

        public PaymentStatus? Status { get; set; }

        // Payment gateway data
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TransactionId { get; set; }

        // Payer info
        [EmailAddress]
        public string? PayerEmail { get; set; }

        [StringLength(200)]
        public string? PayerName { get; set; }

        public DateTime? PaidAt { get; set; }
        public string? FailureReason { get; set; }

        // URLs for redirect
        [Url]
        public string? ReturnUrl { get; set; }

        [Url]
        public string? CancelUrl { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}

