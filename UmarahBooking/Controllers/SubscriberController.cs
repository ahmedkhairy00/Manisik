using Microsoft.AspNetCore.Mvc;
using Manisik.Models;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubscriberController> _logger;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public SubscriberController(IUnitOfWork unitOfWork, ILogger<SubscriberController> logger, IHostEnvironment env, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _env = env;
            _configuration = configuration;
        }

        [HttpPost("Subscribe")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Subscribe([FromBody] SubscriberDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(ApiResponse<string>.ErrorResponse("Invalid email address"));

                var email = dto.Email.Trim().ToLowerInvariant();

                // Basic server-side email format validation
                var emailAttr = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
                if (!emailAttr.IsValid(email))
                    return BadRequest(ApiResponse<string>.ErrorResponse("Invalid email address"));

                // Check if already subscribed
                var existing = await _unitOfWork.Subscribers.FindBySearch(s => s.Email == email);
                if (existing != null)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Email already subscribed"));
                }

                var subscriber = new Subscriber
                {
                    Email = email,
                    IsActive = true,
                    SubscribedAt = DateTime.UtcNow
                };

                await _unitOfWork.Subscribers.AddAsync(subscriber);

                try
                {
                    await _unitOfWork.SaveChanges();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "DB error while subscribing email");
                    var baseMsg = dbEx.GetBaseException()?.Message ?? dbEx.Message;

                    if (baseMsg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                        baseMsg.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                        baseMsg.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase))
                    {
                        return Conflict(ApiResponse<string>.ErrorResponse("Email already subscribed"));
                    }

                    if (_env.IsDevelopment())
                    {
                        return StatusCode(500, ApiResponse<string>.ErrorResponse($"Database error: {baseMsg}"));
                    }

                    return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while processing subscription"));
                }

                _logger.LogInformation("New subscriber added: {Email}", email);

                // Attempt to send confirmation email (non-blocking)
                try
                {
                    var smtpSection = _configuration.GetSection("Smtp");
                    var host = smtpSection["Host"] ?? string.Empty;
                    var port = smtpSection.GetValue<int?>("Port") ?? 587;
                    var username = smtpSection["Username"] ?? string.Empty;
                    var password = smtpSection["Password"] ?? string.Empty; // recommended to set via user-secrets or env var
                    var from = smtpSection["From"] ?? username;
                    var fromName = smtpSection["FromName"] ?? "Manisik";

                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(from))
                    {
                        using var mail = new MailMessage();
                        mail.From = new MailAddress(from, fromName);
                        mail.To.Add(email);
                        mail.Subject = "Subscription confirmed";
                        mail.IsBodyHtml = true;
                        mail.Body = $"<p>Thank you for subscribing to <strong>Manisik</strong> newsletter.</p><p>You will receive updates to: {WebUtility.HtmlEncode(email)}</p>";

                        using var smtp = new SmtpClient(host, port);
                        smtp.EnableSsl = true;

                        if (!string.IsNullOrEmpty(username))
                        {
                            smtp.Credentials = new NetworkCredential(username, password);
                        }

                        // Send synchronously - it's quick; if concerned, move to background job
                        smtp.Send(mail);
                    }
                    else
                    {
                        _logger.LogDebug("SMTP settings missing - skipping send confirmation email");
                    }
                }
                catch (Exception mailEx)
                {
                    // Log but do not fail the subscription if mail sending fails
                    _logger.LogError(mailEx, "Failed to send subscription confirmation email to {Email}", email);
                }

                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Subscribed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while subscribing email");
                if (_env.IsDevelopment())
                    return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.GetBaseException()?.Message ?? ex.Message));

                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while processing subscription"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("List")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubscriberDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> List()
        {
            try
            {
                var subs = await _unitOfWork.Subscribers.FindAllBySearch(s => s.IsActive);
                var result = subs.Select(s => new SubscriberDto { Email = s.Email }).ToList();
                return Ok(ApiResponse<IEnumerable<SubscriberDto>>.SuccessResponse(result, $"{result.Count} subscribers retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while listing subscribers");
                return StatusCode(500, ApiResponse<IEnumerable<SubscriberDto>>.ErrorResponse("An error occurred while retrieving subscribers"));
            }
        }
    }
}
