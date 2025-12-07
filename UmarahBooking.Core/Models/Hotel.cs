using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class Hotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HotelId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]

        public HotelCity HotelCity { get; set; } // Makkah, Madinah

        [Required]
        [MaxLength(250)]
        public string Address { get; set; }

        [Required]
        [Range(1, 5)]
        public int StarRating { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DistanceToHaram { get; set; } // in KM

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(250)]
        public string ImageUrl { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        
        public ICollection<HotelRoom>? Rooms { get; set; }
        public ICollection<BookingHotel> BookingHotels { get; set; }

        public int? CreatedByUserId { get; set; }
    }
}
