using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Manisik.DTOs;

namespace Manisik.Services.Payment
{
    public class StripePaymentService
    {
        private readonly string _secretKey;
        private readonly string _webhookSecret;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration config, ILogger<StripePaymentService> logger)
        {
            _logger = logger;
            _secretKey = config["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey");
            _webhookSecret = config["Stripe:WebhookSecret"] ?? throw new ArgumentNullException("Stripe:WebhookSecret");

            StripeConfiguration.ApiKey = _secretKey;
            _logger.LogInformation("StripePaymentService initialized.");
        }

        public async Task<StripeIntentResponseDto> CreatePaymentIntentAsync(StripeCreateIntentDto dto)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.Amount * 100), // تحويل للعملة بالـcents
                Currency = dto.Currency,
                ReceiptEmail = dto.ReceiptEmail
            };

            try
            {
                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                _logger.LogInformation("Stripe PaymentIntent created: {Id}", intent.Id);

                return new StripeIntentResponseDto
                {
                    ClientSecret = intent.ClientSecret ?? string.Empty,
                    PaymentIntentId = intent.Id
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe PaymentIntent.");
                throw;
            }
        }

        public Stripe.Event ConstructEvent(string json, string sigHeader)
        {
            if (string.IsNullOrEmpty(sigHeader))
                throw new ArgumentException("Stripe signature header is missing.");

            try
            {
                return EventUtility.ConstructEvent(json, sigHeader, _webhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to construct Stripe event.");
                throw;
            }
        }
    }
}
