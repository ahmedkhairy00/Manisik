using System.ComponentModel.DataAnnotations;

namespace UmarahBooking.Core.DTO
{
    public class SubscriberDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
