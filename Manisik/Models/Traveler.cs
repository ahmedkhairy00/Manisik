using Manisik.Enums;

namespace Manisik.Models
{
    public class Traveler
    {
        public int TravelerId { get; set; }
        public int BookingId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; }

        public DateTime PassportExpiryDate { get; set; }
        public string Nationality { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public Booking Booking { get; set; }
    }
}
