using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    public class HotelBookingDto
    {
        [Required]
        public int HotelId { get; set; }

        public string? HotelName { get; set; }

        [Required]
        public int RoomId { get; set; }

        public string? RoomType { get; set; }

        [Required]
        public HotelCity City { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 10)]
        public int NumberOfRooms { get; set; }

        public int? NumberOfNights { get; set; }
        public decimal? PricePerNight { get; set; }
        public decimal? TotalPrice { get; set; }
    }

}

