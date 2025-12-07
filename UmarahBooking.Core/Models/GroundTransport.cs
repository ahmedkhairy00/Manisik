using Manisik.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
    public class GroundTransport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroundTransportId { get; set; }

        public string ?ServiceName { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]

        public InternalTransportType InternalTransportType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerPerson { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(1, 500)]
        public int Capacity { get; set; }

        [Required]
        public bool IsActive { get; set; }
        [Required]
        public string Route { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        public string rate { get; set; }

        // Navigation
        public ICollection<BookingGroundTransport> BookingGroundTransport { get; set; } = new List<BookingGroundTransport>();
    }
}
