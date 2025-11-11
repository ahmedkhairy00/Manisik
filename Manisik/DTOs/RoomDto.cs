using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Manisik.DTOs
{
    public class RoomDto
    {
        public int ID { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        [Required]
        public int HotelId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public List<string> ImgsUrl { get; set; } = new List<string>();
    }
}
