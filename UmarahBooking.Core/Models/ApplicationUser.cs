using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Manisik.Models
{
    // NOTE: Switched ApplicationUser to be Guid-keyed to follow project requirement
    // ApplicationUser now inherits from IdentityUser<Guid>. This will require
    // Identity configuration changes in Program.cs and a database migration.
    public class ApplicationUser : IdentityUser<int>
    {
        // Full name of the user
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = default!;

        // Optional: date of birth for traveler profiles
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // Optional: country of residence
        [MaxLength(100)]
        public string? Country { get; set; }

        // Audit information - set defaults on creation
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation: one user can have many bookings.
        // Kept nullable to avoid EF Core collection initialization warnings.
        public ICollection<Booking>? Bookings { get; set; }

        public ICollection<AIConversation>? AIConversations { get; set; }

    }
}
