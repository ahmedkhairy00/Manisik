using UmarahBooking.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UmarahBooking.Core.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;

        [Required]
        [Column(TypeName = "varchar(50)")]

        public PaymentStatus Status { get; set; } = PaymentStatus.Paid;

        // Important for Stripe / PayPal
        [MaxLength(200)]
        public string? PaymentIntentId { get; set; }   // Stripe PaymentIntent OR PayPal OrderId
        [MaxLength(200)]
        public string? TransactionId { get; set; }     // Stripe ChargeId OR PayPal CaptureId
        [MaxLength(250)]
        public string? PayerEmail { get; set; }
        [MaxLength(250)]
        public string? PayerName { get; set; }

        public DateTime? PaidAt { get; set; }
        public string? FailureReason { get; set; }

        // Idempotency key provided by frontend to protect against retries
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties - initialize collections to avoid null refs 
        // every payment can have multiple events (created, succeeded, failed, refunded, etc)
        public ICollection<PaymentEvent> PaymentEvents { get; set; } = new List<PaymentEvent>();
        public string ClientSecret { get; set; }
    }
}

