using Manisik.Enums;
using System.ComponentModel.DataAnnotations;

namespace Manisik.Models
{
    public class HotelRoom
    {
        [Key]
        public int HotelRoomID { get; set; }

        [Required]
        public RoomType RoomType { get; set; }

        public int? Capacity { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }
        public int? TotalRooms { get; set; }
        public int? AvailableRooms { get; set; }

        public bool? IsActive { get; set; }
        
        public string? ImageUrl { get; set; }

        [Required]
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }
      



    }
}
