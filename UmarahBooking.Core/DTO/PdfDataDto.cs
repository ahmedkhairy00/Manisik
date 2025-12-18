namespace UmarahBooking.Core.DTO
{
    /// <summary>
    /// Data for generating Visa PDF document
    /// </summary>
    public class VisaPdfData
    {
        // Personal Information
        public string FullName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }
        
        /// <summary>
        /// Photo bytes for embedding in PDF (optional, loaded from PhotoUrl by controller)
        /// </summary>
        public byte[]? PhotoBytes { get; set; }

        // Visa Information
        public string VisaType { get; set; } = "Umrah";
        public DateTime VisaExpiryDate { get; set; }
        public int StayDuration { get; set; } = 30; // days
        public int EntryCount { get; set; } = 1;
        public string IssuingAuthority { get; set; } = "Manisik";
        public string BookingNumber { get; set; } = string.Empty;
        
        // Travel Dates
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }
    }

    /// <summary>
    /// Data for generating Ticket PDF document
    /// </summary>
    public class TicketPdfData
    {
        // Personal Information
        public string FullName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string VisaType { get; set; } = "Umrah";

        // Outbound Flight Information
        public string FlightNumber { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public string? SeatNumber { get; set; }
        
        // Return Flight Information
        public DateTime? ReturnDate { get; set; }
        public string? ReturnFlightNumber { get; set; }
        
        public string BookingNumber { get; set; } = string.Empty; // Used as PNR
    }
}
