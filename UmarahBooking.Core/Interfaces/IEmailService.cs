using System;
using System.Threading.Tasks;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string content);
        Task SendWelcomeEmailAsync(string to);
        Task SendBookingStatusUpdateAsync(string to, string bookingNumber, string status, string travelerName, string bookingType);
        Task SendPaymentSuccessEmailAsync(string to, string bookingNumber, decimal amount, string customerName, DateTime travelStart, DateTime travelEnd, string tripType);
        Task SendDetailedPaymentReceiptAsync(PaymentReceiptDto receipt);
        Task SendBroadcastEmailAsync(string to, string subject, string bodyContent);
    }
}
