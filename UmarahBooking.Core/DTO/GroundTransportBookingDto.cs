using UmarahBooking.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{

    /// <summary>
    /// Ground transport booking details
    /// </summary>
    public class GroundTransportBookingDto
    {

        [Required]
        public int GroundTransportId { get; set; }

        public string? ServiceName { get; set; }
        public InternalTransportType? Type { get; set; }

        [Required]
        public DateTime ServiceDate { get; set; }

        [Required]
        [StringLength(200)]
        public string PickupLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DropoffLocation { get; set; } = string.Empty;

        [Range(1, 50)]
        public int NumberOfPassengers { get; set; }

        public decimal? PricePerPerson { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}

