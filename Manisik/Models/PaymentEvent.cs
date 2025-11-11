using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class PaymentEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Provider { get; set; } = string.Empty; // "Stripe" or "PayPal"

        [Required]
        public string EventId { get; set; } = string.Empty; // webhook event id or capture id

        public DateTime ProcessedAt { get; set; }

        public int? BookingId { get; set; }
    }
}
