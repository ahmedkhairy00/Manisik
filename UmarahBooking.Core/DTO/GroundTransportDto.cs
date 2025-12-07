using Manisik.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmarahBooking.Core.DTO
{
    public class GroundTransportDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ServiceNameAr { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal PricePerPerson { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? DescriptionAr { get; set; }

        [Required]
        [Range(1, 100)]
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public string Route { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        public string rate { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}


