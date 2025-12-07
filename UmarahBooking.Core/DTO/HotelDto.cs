using System.ComponentModel.DataAnnotations;

namespace UmarahBooking.Core.DTO
{
    /// <summary>
    /// Unified Hotel DTO - handles ALL scenarios:
    /// - List view (rooms = null)
    /// - Detail view (rooms populated)
    /// - Create (Id = null)
    /// - Update (Id required)
    public class HotelDto
    {  // ========== IDENTITY ==========
        /// <summary>
        /// Hotel ID - NULL for create, REQUIRED for update
        /// </summary>
        public int? Id { get; set; }

        // ========== BASIC INFO ==========
        [Required(ErrorMessage = "Hotel name is required")]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;


        [Required]
        public string City { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;


        [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5")]
        public int StarRating { get; set; }

        [Range(0.0, 50.0, ErrorMessage = "Distance must be between 0 and 50 km")]
        public decimal DistanceToHaram { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? DescriptionAr { get; set; }

        public string? ImageUrl { get; set; }

        public decimal PricePerNight { get; set; }
        public int AvailableRooms { get; set; }

        // ========== ROOMS (Nullable - populated in detail view only) ==========
        /// <summary>
        /// NULL in list view, POPULATED in detail view
        /// Frontend checks: if (hotel.Rooms != null) { show details }
        /// </summary>
        public List<RoomDto>? Rooms { get; set; }

        // ========== METADATA (Read-only) ==========
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
    }


}
