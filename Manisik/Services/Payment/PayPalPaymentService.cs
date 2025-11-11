using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Manisik.DTOs;

namespace Manisik.Services.Payment
{
    public class PayPalPaymentService
    {
        private readonly HttpClient _http;
        private readonly string _clientId;
        private readonly string _secret;
        private readonly string _apiBase;
        private readonly ILogger<PayPalPaymentService> _logger;

        private string? _accessToken;
        private DateTime _accessTokenExpiresAt = DateTime.MinValue;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);

        public PayPalPaymentService(HttpClient http, IConfiguration config, ILogger<PayPalPaymentService> logger)
        {
            _http = http;
            _logger = logger;

            _clientId = config["PayPal:ClientId"] ?? throw new ArgumentNullException("PayPal:ClientId");
            _secret = config["PayPal:Secret"] ?? throw new ArgumentNullException("PayPal:Secret");
            var mode = (config["PayPal:Mode"] ?? "Sandbox").ToLowerInvariant();

            _apiBase = mode == "live"
                ? "https://api-m.paypal.com"
                : "https://api-m.sandbox.paypal.com";

            _logger.LogInformation("PayPalPaymentService initialized in {Mode} mode.", mode);
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _accessTokenExpiresAt > DateTime.UtcNow.AddSeconds(30))
                return _accessToken;

            await _tokenLock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && _accessTokenExpiresAt > DateTime.UtcNow.AddSeconds(30))
                    return _accessToken;

                var url = $"{_apiBase}/v1/oauth2/token";
                var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "client_credentials" })
                };

                var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}"));
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

                var resp = await _http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to obtain PayPal token. Status: {Status}. Body: {Body}", resp.StatusCode, body);
                    return null;
                }

                using var doc = JsonDocument.Parse(body);
                _accessToken = doc.RootElement.GetProperty("access_token").GetString();
                var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
                _accessTokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

                _logger.LogInformation("PayPal access token acquired, expires in {Seconds} seconds.", expiresIn);
                return _accessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        public async Task<PayPalOrderResponseDto?> CreateOrderAsync(PayPalCreateOrderDto dto)
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var url = $"{_apiBase}/v2/checkout/orders";

            var payload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new { amount = new { currency_code = dto.Currency, value = dto.Amount.ToString("F2", CultureInfo.InvariantCulture) } }
                },
                application_context = new
                {
                    return_url = dto.ReturnUrl,
                    cancel_url = dto.CancelUrl
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            var respBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal CreateOrder failed. Status: {Status}. Body: {Body}", resp.StatusCode, respBody);
                return null;
            }

            using var doc = JsonDocument.Parse(respBody);
            var root = doc.RootElement;
            var id = root.GetProperty("id").GetString() ?? string.Empty;
            string approveLink = string.Empty;

            if (root.TryGetProperty("links", out var links) && links.ValueKind == JsonValueKind.Array)
            {
                foreach (var l in links.EnumerateArray())
                {
                    if (l.TryGetProperty("rel", out var rel) && rel.GetString() == "approve")
                    {
                        approveLink = l.GetProperty("href").GetString() ?? string.Empty;
                        break;
                    }
                }
            }

            _logger.LogInformation("PayPal order created: {OrderId}", id);
            return new PayPalOrderResponseDto { OrderId = id, ApproveLink = approveLink };
        }

        public async Task<string?> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var url = $"{_apiBase}/v2/checkout/orders/{orderId}/capture";
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal CaptureOrder failed. Status: {Status}. Body: {Body}", resp.StatusCode, body);
                return null;
            }

            _logger.LogInformation("PayPal order captured: {OrderId}", orderId);
            return body;
        }
    }
}
