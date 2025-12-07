using Newtonsoft.Json;

namespace UmarahBooking.Core.DTO
{
    public class CreatePaymentResponseDto
    {
        [JsonProperty("clientSecret")]
        public string? ClientSecret { get; set; }

        [JsonProperty("paymentIntentId")]
        public string? PaymentIntentId { get; set; }
    }
}
