namespace UmarahBooking.Core.Enums
{
    public enum PaymentStatus
    {
        Pending,   // Payment is pending
        Confirmed, // Payment is confirmed
        Cancelled, // Payment is cancelled
        Refunded,  // Payment is refunded
        Paid,    // Payment completed successfully
        Failed,
        Completed,
    }
}

