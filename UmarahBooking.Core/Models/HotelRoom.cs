using UmarahBooking.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UmarahBooking.Core.Models
{
    public class HotelRoom
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HotelRoomId { get; set; }

        [Required]
        public RoomType RoomType { get; set; }

        [Required]
        [Range(1, 10)]
        public int Capacity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Required]
        [Range(0, 500)]
        public int AvailableRooms { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // FK & Navigation
        [Required]
        public int HotelId { get; set; }
        [ForeignKey(nameof(HotelId))]
        public Hotel Hotel { get; set; }
    }
}

