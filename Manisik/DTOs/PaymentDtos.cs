using System;

namespace Manisik.DTOs
{
    // Stripe
    public class StripeCreateIntentDto
    {
        // amount in smallest currency unit (e.g. cents; for SAR use halala)
        public long Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string? ReceiptEmail { get; set; }
    }

    public class StripeIntentResponseDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
    }

    // PayPal
    public class PayPalCreateOrderDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }

    public class PayPalOrderResponseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string ApproveLink { get; set; } = string.Empty;
    }
}