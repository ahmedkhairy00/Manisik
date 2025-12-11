using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string content)
        {
            try
            {
                var smtpSection = _configuration.GetSection("Smtp");
                var host = smtpSection["Host"];
                var port = smtpSection.GetValue<int>("Port");
                var username = smtpSection["Username"];
                var password = smtpSection["Password"];
                var from = smtpSection["From"];
                var fromName = smtpSection["FromName"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("SMTP configuration is missing. Email to {To} not sent.", to);
                    return;
                }

                using var mail = new MailMessage();
                mail.From = new MailAddress(from!, fromName);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = content;
                mail.IsBodyHtml = true;

                using var smtp = new SmtpClient(host, port);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(username, password);

                await smtp.SendMailAsync(mail);
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                // We don't throw to avoid breaking the main flow
            }
        }

        public async Task SendWelcomeEmailAsync(string to)
        {
            var subject = "Welcome to Manisik Newsletter!";
            var body = GetHtmlTemplate("Welcome to Manisik!", 
                @"<p style='margin: 0 0 16px 0;'>Hi there,</p>
<p style='margin: 0 0 16px 0;'>Thank you for subscribing to our newsletter. You'll now receive the latest updates and travel offers from <strong>Manisik</strong>.</p>
<p style='margin: 0;'>We are excited to have you with us.</p>");

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendBookingStatusUpdateAsync(string to, string bookingNumber, string status, string travelerName, string bookingType)
        {
            var subject = $"Your {bookingType} Booking #{bookingNumber} Status Update";
            var statusColor = status.ToLower() switch
            {
                "confirmed" => "#10B981", // Green
                "cancelled" => "#EF4444", // Red
                "pending" => "#F59E0B",   // Amber
                "paid" => "#3B82F6",      // Blue
                "refunded" => "#8B5CF6",  // Purple
                _ => "#6B7280"            // Gray
            };
            
            var statusEmoji = status.ToLower() switch
            {
                "confirmed" => "✅",
                "cancelled" => "❌",
                "pending" => "⏳",
                "paid" => "💳",
                "refunded" => "💸",
                _ => "📋"
            };
            
            var statusMessage = status.ToLower() switch
            {
                "confirmed" => "Great news! Your booking has been confirmed. We're excited to be part of your spiritual journey!",
                "cancelled" => "Your booking has been cancelled. If you have any questions or need assistance with a new booking, please don't hesitate to contact us.",
                "pending" => "Your booking is currently being reviewed by our team. We'll update you soon!",
                "paid" => "Thank you! Your payment has been received and your booking is now being processed.",
                "refunded" => "Your refund has been processed. The amount will be credited to your account within 5-10 business days.",
                _ => "Your booking status has been updated."
            };
            
            var tripIcon = bookingType.ToLower() == "hajj" ? "🕋" : "🕌";

            var body = GetHtmlTemplate($"Booking Update - {bookingType}",
$@"<p style='font-size: 18px; margin: 0 0 20px 0;'>Dear <strong>{travelerName}</strong>,</p>
<p style='font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>{statusMessage}</p>
<div style='margin: 30px 0; padding: 25px; background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%); border-radius: 12px; border-left: 4px solid {statusColor};'>
<table style='width: 100%; border-collapse: collapse;'>
<tr>
<td style='padding: 8px 0;'>
<span style='color: #6b7280; font-size: 14px;'>Trip Type</span><br/>
<span style='font-size: 18px; font-weight: 600;'>{tripIcon} {bookingType}</span>
</td>
<td style='padding: 8px 0; text-align: right;'>
<span style='color: #6b7280; font-size: 14px;'>Booking Reference</span><br/>
<span style='font-size: 18px; font-weight: 600;'>#{bookingNumber}</span>
</td>
</tr>
</table>
<div style='margin-top: 20px; padding-top: 15px; border-top: 1px solid #dee2e6; text-align: center;'>
<span style='font-size: 14px; color: #6b7280;'>Current Status</span>
<div style='margin-top: 8px;'>
<span style='display: inline-block; padding: 10px 25px; background-color: {statusColor}; color: white; border-radius: 25px; font-size: 16px; font-weight: 600;'>
{statusEmoji} {status}
</span>
</div>
</div>
</div>
<p style='font-size: 15px; color: #6b7280; margin: 0 0 20px 0;'>You can view the full details of your booking in your dashboard.</p>
<div style='text-align: center; margin-top: 30px;'>
<a href='http://localhost:4200/dashboard' style='display: inline-block; background: linear-gradient(135deg, #2563EB 0%, #1d4ed8 100%); color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 6px rgba(37, 99, 235, 0.3);'>View Your Dashboard</a>
</div>
<div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; text-align: center;'>
<p style='font-size: 14px; color: #9ca3af; margin: 0;'>Need help? Contact us at <a href='mailto:manisik.info@gmail.com' style='color: #2563EB;'>manisik.info@gmail.com</a></p>
</div>");

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPaymentSuccessEmailAsync(string to, string bookingNumber, decimal amount, string customerName, DateTime travelStart, DateTime travelEnd, string tripType)
        {
            var subject = $"Payment Receipt for Booking #{bookingNumber}";
            
            // Format dates
            var dateStr = DateTime.Now.ToString("MMM d, yyyy, h:mm:ss tt");
            var tripStr = $"{travelStart:MMM d, yyyy} - {travelEnd:MMM d, yyyy}";
            var tripIcon = tripType.ToLower() == "hajj" ? "🕋" : "🕌";

            var content = $@"<div style='background-color: white; padding: 0; border-radius: 8px;'>
<h2 style='color: #000; margin: 0 0 30px 0; text-align: center; font-size: 24px;'>Payment Successful</h2>
<table style='width: 100%; border-collapse: collapse; margin-bottom: 30px; color: #000;'>
<tr>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee;'><strong>Paid By / Account Holder</strong></td>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee; text-align: right;'>{customerName}<br><span style='font-size: 13px; color: #555;'>{to}</span></td>
</tr>
<tr>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee;'><strong>Booking Reference</strong></td>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee; text-align: right; font-weight: 600;'>{bookingNumber}</td>
</tr>
<tr>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee;'><strong>Trip Type</strong></td>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee; text-align: right;'><span style='display: inline-block; padding: 4px 12px; background-color: #f3f4f6; border-radius: 4px; font-weight: 600;'>{tripIcon} {tripType}</span></td>
</tr>
<tr>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee;'><strong>Trip Dates</strong></td>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee; text-align: right;'>{tripStr}</td>
</tr>
<tr>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee;'><strong>Payment Date</strong></td>
<td style='padding: 15px 10px; border-bottom: 1px solid #eee; text-align: right;'>{dateStr}</td>
</tr>
</table>
<div style='background-color: #f8f9fa; padding: 30px; margin: 30px 0; border-radius: 12px; text-align: center;'>
<p style='margin: 0; color: #555; font-size: 14px; text-transform: uppercase; letter-spacing: 1px;'>Total Amount Paid</p>
<h1 style='margin: 10px 0 0; color: #000; font-size: 36px; font-weight: 700;'>${amount:N2}</h1>
</div>
<div style='text-align: center; margin-top: 40px;'>
<a href='http://localhost:4200/dashboard' style='display: inline-block; background-color: #000; color: #fff; padding: 14px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px;'>View Booking Details</a>
</div>
</div>";

            var body = GetHtmlTemplate("Payment Successful", content);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendBroadcastEmailAsync(string to, string subject, string bodyContent)
        {
            var body = GetHtmlTemplate(subject, bodyContent);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendDetailedPaymentReceiptAsync(PaymentReceiptDto receipt)
        {
            var subject = $"Payment Receipt - {receipt.BookingNumber}";
            
            // Format dates
            var dateStr = receipt.PaymentDate.ToString("MMM d, yyyy, h:mm:ss tt");
            var tripStr = $"{receipt.TripStartDate:MMM d, yyyy} - {receipt.TripEndDate:MMM d, yyyy}";

            var itemsHtml = "";
            foreach (var item in receipt.Items)
            {
                var detailsHtml = "";
                foreach (var detail in item.Details)
                {
                    detailsHtml += $"<div style='color: #6b7280; font-size: 14px;'>{detail}</div>";
                }

                itemsHtml += $@"<div style='padding: 15px 0; border-bottom: 1px solid #f3f4f6;'>
<table style='width: 100%; border-collapse: collapse;'>
<tr>
<td style='vertical-align: top;'>
<h4 style='margin: 0 0 5px 0; color: #111827; font-size: 16px;'>{item.Title}</h4>
{detailsHtml}
</td>
<td style='text-align: right; vertical-align: top;'>
<span style='font-weight: 600; color: #111827;'>${item.Amount:N2}</span>
</td>
</tr>
</table>
</div>";
            }

            var content = $@"<div style='background-color: #ffffff; padding: 0;'>
<div style='text-align: center; margin-bottom: 30px;'>
<div style='display: inline-block; padding: 12px; background-color: #ecfdf5; border-radius: 50%; margin-bottom: 15px;'>
<span style='font-size: 24px;'>✓</span>
</div>
<h2 style='color: #111827; margin: 0; font-size: 24px; font-weight: 700;'>Payment Receipt</h2>
<p style='color: #6b7280; margin: 5px 0 0;'>Thank you for your booking</p>
</div>
<div style='margin-bottom: 30px;'>
<h3 style='color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 10px 0;'>Customer Details</h3>
<div style='background-color: #f9fafb; border-radius: 8px; padding: 15px;'>
<div style='font-weight: 600; color: #111827; font-size: 16px;'>{receipt.CustomerName}</div>
<div style='color: #6b7280; font-size: 14px; margin-top: 2px;'>{receipt.CustomerEmail}</div>
</div>
</div>
<table style='width: 100%; border-collapse: collapse; margin-bottom: 30px;'>
<tr>
<td style='vertical-align: top; width: 50%; padding-right: 10px;'>
<h3 style='color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 8px 0;'>Booking Reference</h3>
<div style='color: #111827; font-weight: 600; font-size: 16px;'>{receipt.BookingNumber}</div>
</td>
<td style='vertical-align: top; width: 50%; padding-left: 10px;'>
<h3 style='color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 8px 0;'>Date</h3>
<div style='color: #111827; font-weight: 500; font-size: 14px;'>{dateStr}</div>
</td>
</tr>
<tr>
<td style='vertical-align: top; padding-top: 20px; padding-right: 10px;'>
<h3 style='color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 8px 0;'>Trip</h3>
<div style='color: #111827; font-weight: 500; font-size: 14px;'>
<span style='display: inline-block; padding: 6px 12px; background-color: #dbeafe; color: #1e40af; border-radius: 6px; font-size: 14px; margin-bottom: 8px; font-weight: 700;'>{receipt.TripType}</span><br>
{tripStr}
</div>
</td>
<td style='vertical-align: top; padding-top: 20px; padding-left: 10px;'>
<h3 style='color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 8px 0;'>Payment Method</h3>
<div style='color: #111827; font-weight: 500; font-size: 14px;'>{receipt.PaymentMethod}</div>
<div style='color: #6b7280; font-size: 12px; margin-top: 4px;'>Transaction ID: {receipt.TransactionId}</div>
</td>
</tr>
</table>
<div style='margin-bottom: 30px;'>
<h3 style='color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 15px 0; border-bottom: 1px solid #e5e7eb; padding-bottom: 10px;'>Detailed Breakdown</h3>
{itemsHtml}
<div style='padding: 15px 0; border-bottom: 1px solid #e5e7eb;'>
<table style='width: 100%; border-collapse: collapse;'>
<tr>
<td style='padding-bottom: 5px;'>
<h4 style='margin: 0; color: #4b5563; font-size: 14px; font-weight: 500;'>Tax (5%)</h4>
</td>
<td style='text-align: right; padding-bottom: 5px;'>
<span style='font-weight: 600; color: #4b5563; font-size: 14px;'>${receipt.Tax:N2}</span>
</td>
</tr>
<tr>
<td>
<h4 style='margin: 0; color: #4b5563; font-size: 14px; font-weight: 500;'>Service Fees</h4>
</td>
<td style='text-align: right;'>
<span style='font-weight: 600; color: #4b5563; font-size: 14px;'>${receipt.OtherFees:N2}</span>
</td>
</tr>
</table>
</div>
<div style='padding: 20px 0 0;'>
<table style='width: 100%; border-collapse: collapse;'>
<tr>
<td>
<h4 style='margin: 0; color: #111827; font-size: 18px; font-weight: 700;'>Amount Paid</h4>
</td>
<td style='text-align: right;'>
<span style='font-weight: 700; color: #059669; font-size: 24px;'>${receipt.TotalAmount:N2}</span>
</td>
</tr>
</table>
</div>
</div>
<div style='text-align: center; margin-top: 30px;'>
<a href='http://localhost:4200/dashboard' style='display: inline-block; background-color: #111827; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 14px;'>View Booking Dashboard</a>
</div>
</div>";

            var body = GetHtmlTemplate("Payment Receipt", content);
            await SendEmailAsync(receipt.CustomerEmail, subject, body);
        }

        private string GetHtmlTemplate(string title, string content)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>{title}</title>
</head>
<body style='font-family: ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5; color: #000000; padding: 40px 20px; margin: 0;'>
<div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border: 1px solid #eee; border-radius: 12px; overflow: hidden;'>
<div style='padding: 30px; text-align: center; border-bottom: 1px solid #eee;'>
<table align='center' style='margin: 0 auto 15px auto;'>
<tr>
<td style='width: 60px; height: 60px; background-color: #000; border-radius: 12px; text-align: center; vertical-align: middle; font-size: 30px;'>🕋</td>
</tr>
</table>
<h1 style='color: #000000; margin: 0; font-size: 24px; font-weight: 700;'>Manisik</h1>
</div>
<div style='padding: 40px 30px; color: #000000;'>
{content}
</div>
<div style='background-color: #ffffff; padding: 20px; text-align: center; font-size: 12px; color: #666666; border-top: 1px solid #eeeeee;'>
<p style='margin: 0 0 10px 0; color: #666666;'>&copy; {DateTime.Now.Year} Manisik Inc. All rights reserved.</p>
<p style='margin: 0; color: #666666;'>123 Travel Way, World &bull; <a href='mailto:manisik.info@gmail.com' style='color: #000000; text-decoration: none;'>manisik.info@gmail.com</a></p>
</div>
</div>
</body>
</html>";
        }
    }
}
