using Microsoft.AspNetCore.Mvc;
using UmarahBooking.Core.Models;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace UmarahBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubscriberController> _logger;
        private readonly IHostEnvironment _env;
        private readonly IEmailService _emailService;

        public SubscriberController(IUnitOfWork unitOfWork, ILogger<SubscriberController> logger, IHostEnvironment env, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _env = env;
            _emailService = emailService;
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
                _ = _emailService.SendWelcomeEmailAsync(email);

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
        [Authorize(Roles = "Admin")]
        [HttpPost("Broadcast")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Subject) || string.IsNullOrWhiteSpace(dto.Body))
                    return BadRequest(ApiResponse<string>.ErrorResponse("Subject and Body are required"));

                // 1. Get all active subscribers
                var subscribers = await _unitOfWork.Subscribers.FindAllBySearch(s => s.IsActive);
                var subEmails = subscribers.Select(s => s.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

                // 2. Get all users
                var users = await _unitOfWork.Context.Set<ApplicationUser>().ToListAsync();
                foreach (var u in users)
                {
                    if (!string.IsNullOrEmpty(u.Email))
                        subEmails.Add(u.Email);
                }

                // 3. Send Emails (Fire and forget batch)
                _ = Task.Run(async () =>
                {
                    foreach (var email in subEmails)
                    {
                        await _emailService.SendBroadcastEmailAsync(email, dto.Subject, dto.Body);
                        // Tiny delay to be nice to SMTP server
                        await Task.Delay(50); 
                    }
                    _logger.LogInformation("Broadcast sent to {Count} recipients", subEmails.Count);
                });

                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, $"Broadcast queued for {subEmails.Count} recipients"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing broadcast");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to queue broadcast"));
            }
        }
    }

    public class BroadcastDto
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    }


