using UmarahBooking.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace UmarahBooking.Core.DTO
{
    /// <summary>
    /// Unified Booking DTO - handles create, update, and response
    /// </summary>
    public class BookingDto
    {
        // ========== IDENTITY ==========
        public int Id { get; set; }
        public string? BookingNumber { get; set; }

        // ========== USER ==========
        public int? UserId { get; set; }
        public UserDto? User { get; set; }

        // ========== BOOKING TYPE ==========
        [Required]
        public string Type { get; set; }

        [Required]
        public string Status { get; set; } = BookingStatus.Pending.ToString();

        [Required]
        public DateTime TravelStartDate { get; set; }

        public DateTime? TravelEndDate { get; set; }

        [Range(1, 50)]
        public int NumberOfTravelers { get; set; }

        // ========== COMPONENTS (Nullable - populated as user progresses) ==========
        /// <summary>
        /// Makkah hotel - populated in step 1
        /// </summary>
        public HotelBookingDto? MakkahHotel { get; set; }

        /// <summary>
        /// Madinah hotel - populated in step 2
        /// </summary>
        public HotelBookingDto? MadinahHotel { get; set; }

        /// <summary>
        /// International transport - populated in step 3
        /// </summary>
        public TransportBookingDto? InternationalTransport { get; set; }

        /// <summary>
        /// Ground transport (optional)
        /// </summary>
        public GroundTransportBookingDto? GroundTransport { get; set; }

        /// <summary>
        /// Travelers - populated in step 4
        /// </summary>
        public List<TravelerDto>? Travelers { get; set; }

        /// <summary>
        /// Payment info - populated in step 5
        /// </summary>
        public PaymentDto? Payment { get; set; }

        // ========== PRICING ==========
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? TotalPrice { get; set; }

        // ========== METADATA ==========
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ========== VALIDATION HELPERS ==========
        /// <summary>
        /// Check if booking is ready for submission
        /// </summary>
        /// 
        public bool IsComplete()
        {
            return MakkahHotel != null &&
                   MadinahHotel != null &&
                   InternationalTransport != null &&
                   Travelers != null &&
                   Travelers.Any(); // ? Just check travelers exist
        }

        /// <summary>
        /// Get current step number
        /// </summary>
        public int GetCurrentStep()
        {
            if (MakkahHotel == null) return 1;
            if (MadinahHotel == null) return 2;
            if (InternationalTransport == null) return 3;
            if (GroundTransport == null) return 4; // Optional but in sequence
            if (Travelers == null || !Travelers.Any()) return 5;
            return 6; // Complete (payment happens AFTER booking creation)
        }
    }
}

