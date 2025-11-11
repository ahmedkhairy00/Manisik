using System.ComponentModel.DataAnnotations;

namespace Manisik.Models
{
    public class Room
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty;
        [Required]
        public int Capacity { get; set; }
        [Required]
        public decimal PricePerNight { get; set; }
        [Required]
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }
        public List<string> ImgsUrl { get; set; } = new List<string>();

        // Availability flag (calculated/updated before response)
        public bool IsAvailable { get; set; } = true;

    }
}
