using System;
using System.Collections.Generic;

namespace UmarahBooking.Core.DTO
{
    public class PaymentReceiptDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Card"; // e.g., "STRIPE •••• 4242"
        public DateTime PaymentDate { get; set; }
        public DateTime TripStartDate { get; set; }
        public DateTime TripEndDate { get; set; }
        public string TripType { get; set; } = string.Empty; // "Umrah" or "Hajj"
        
        public List<ReceiptItemDto> Items { get; set; } = new List<ReceiptItemDto>();
        public decimal Tax { get; set; }        // Explicit Tax (e.g. 15% or 5%)
        public decimal OtherFees { get; set; }  // Remaining Service Fees
        public decimal TotalAmount { get; set; }
    }

    public class ReceiptItemDto
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Details { get; set; } = new List<string>(); // Lines like "Check-in: Dec 11", "Rooms: 1"
        public decimal Amount { get; set; }
    }
}
