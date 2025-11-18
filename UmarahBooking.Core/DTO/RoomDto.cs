using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    /// <summary>
    /// Unified Room DTO
    /// </summary>
    public class RoomDto
    {
        public int? Id { get; set; }

        [Required]
        public int HotelId { get; set; }

        [Required]
        [StringLength(50)]
        public RoomType RoomType { get; set; } = RoomType.Single;

        [Range(1, 10)]
        public int Capacity { get; set; }

        [Range(0.01, 100000)]
        public decimal PricePerNight { get; set; }

        [Range(0, 1000)]
        public int TotalRooms { get; set; }

        [Range(0, 1000)]
        public int AvailableRooms { get; set; }

        public bool IsActive { get; set; } = true;
    }

}

