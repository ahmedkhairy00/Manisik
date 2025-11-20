using Manisik.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class BookingHotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingHotelId { get; set; }

        // Foreign Keys
        [Required]
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }

        [Required]
        public int HotelId { get; set; }
        [ForeignKey(nameof(HotelId))]
        public Hotel Hotel { get; set; }

        [Required]
        public int RoomId { get; set; }
        [ForeignKey(nameof(RoomId))]
        public HotelRoom Room { get; set; }

        public HotelCity? City { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 100)]
        public int NumberOfRooms { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        //calculated column ----------
        public decimal TotalPrice { get; set; }
    }
}
