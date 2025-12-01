using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class BookingInternationalTransport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingInternationalTransportId { get; set; }

        [Required]
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        
        public Booking Booking { get; set; }

        [Required]
        public int InternationalTransportId { get; set; }
        [ForeignKey(nameof(InternationalTransportId))]
        public InternationalTransport InternationalTransport { get; set; }

        [Required]
        [Range(1, 500)]
        public int NumberOfSeats { get; set; }



        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
    }
}
