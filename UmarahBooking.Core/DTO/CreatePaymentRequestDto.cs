namespace UmarahBooking.Core.DTO
{
    public class CreatePaymentRequestDto
    {
        public int BookingId { get; set; }
        public string? Currency { get; set; } = "usd";
        public decimal? Amount { get; set; }
        public string? IdempotencyKey { get; set; }
    }
}
