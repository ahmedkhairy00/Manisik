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

        // PaymentId For which payment this event is related to
        [Required]
        public int PaymentId { get; set; }
        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }

        // Provider name e.g., "Stripe", "PayPal"
        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = string.Empty;

        // Provider event id (stripe event id or paypal webhook id)
        [MaxLength(200)]
        public string? EventId { get; set; }

        

        [Required]
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        public string? Payload { get; set; }
    }
}
