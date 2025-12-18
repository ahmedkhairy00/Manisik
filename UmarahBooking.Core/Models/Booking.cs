using UmarahBooking.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UmarahBooking.Core.Models
{
    // Booking aggregate representing a customer's Umrah/Hajj booking
    public class Booking
    {
        // Primary key for booking - currently integer; a future migration can move domain PKs to Guid
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

        // When the booking was created
        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        // FK to ApplicationUser - migrated to Guid to match ApplicationUser's key
        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        //---------------nullable columns---------------------------

        // Human-friendly booking number, e.g. BK-2025-0001
        [MaxLength(20)]
        public string? BookingNumber { get; set; } = default!;


        [Column(TypeName = "varchar(50)")]
        public TripType TripType { get; set; }  // Umrah or Hajj

        // Total price for the whole booking and Fees - stored as decimal(18,2)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ServiceFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalPrice { get; set; }



        // Travel window

        [DataType(DataType.Date)]
        public DateTime? TravelStartDate { get; set; }


        [DataType(DataType.Date)]
        public DateTime? TravelEndDate { get; set; }


        [Range(1, 100)]
        public int? NumberOfTravelers { get; set; }

        // Payment Info
        [Column(TypeName = "varchar(50)")]

        public PaymentStatus? PaymentStatus { get; set; }

        [Column(TypeName = "varchar(50)")]

        public PaymentMethod? PaymentMethod { get; set; }

        // Provider-specific identifier (Stripe PaymentIntent id or PayPal Order id)
        public string? PaymentIntentId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        // Reservation TTL - when a Pending booking reservation expires it should be cancelled by background job
        [DataType(DataType.Date)]
        public DateTime? ReservedUntil { get; set; }

        // Audit timestamps
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }



        // Navigation properties - initialize collections to avoid null refs
        public ICollection<BookingHotel> Hotels { get; set; } = new List<BookingHotel>();
        public ICollection<Traveler> Travelers { get; set; } = new List<Traveler>();
        public ICollection<BookingInternationalTransport> BookingInternationalTransport { get; set; } = new List<BookingInternationalTransport>();
        public ICollection<BookingGroundTransport> BookingGroundTransport { get; set; } = new List<BookingGroundTransport>();

        // One-to-one payment record
        public Payment? Payment { get; set; }
    }
}

