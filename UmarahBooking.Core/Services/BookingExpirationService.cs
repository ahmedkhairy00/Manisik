using System;
using System.Collections.Generic;
using System.Linq;
using UmarahBooking.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using UmarahBooking.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UmarahBooking.Core.Services
{
    // Background service that expires short-lived pending bookings and restores room availability
    public class BookingExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingExpirationService> _logger;
        private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(1);

        public BookingExpirationService(IServiceScopeFactory scopeFactory, ILogger<BookingExpirationService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingExpirationService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireBookingsAsync(stoppingToken);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while expiring bookings");
                }

                await Task.Delay(_scanInterval, stoppingToken);
            }

            _logger.LogInformation("BookingExpirationService stopping");
        }

        private async Task ExpireBookingsAsync(CancellationToken cancellationToken)
        {
            // DISABLED: Pending bookings now persist until user completes or manually deletes them
            // This allows users to resume their booking across sessions (days/weeks later)
            // The old behavior would cancel pending bookings after ReservedUntil timer expired
            
            // If you want to re-enable expiration in the future, uncomment the code below:
            /*
            var now = DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredBookings = await unitOfWork.Bookings
                .GetAllAsQuerable()
                .Where(b => b.BookingStatus == UmarahBooking.Core.Enums.BookingStatus.Pending && b.ReservedUntil != null && b.ReservedUntil < now)
                .ToListAsync(cancellationToken);

            foreach (var booking in expiredBookings)
            {
                try
                {
                    await unitOfWork.BeginTransaction();

                    var bookingItems = await unitOfWork.BookingHotels
                        .GetAllAsQuerable()
                        .Where(bh => bh.BookingId == booking.BookingId)
                        .ToListAsync(cancellationToken);

                    foreach (var item in bookingItems)
                    {
                        var room = await unitOfWork.HotelRooms.GetByIdAsync(item.RoomId);
                        if (room != null)
                        {
                            room.AvailableRooms += item.NumberOfRooms;
                            room.IsActive = room.AvailableRooms > 0;
                            await unitOfWork.HotelRooms.UpdateAsync(room);
                        }
                        await unitOfWork.BookingHotels.DeleteAsync(item);
                    }

                    booking.BookingStatus = UmarahBooking.Core.Enums.BookingStatus.Cancelled;
                    await unitOfWork.Bookings.UpdateAsync(booking);

                    await unitOfWork.SaveChanges();
                    await unitOfWork.CommitTransaction();

                    _logger.LogInformation("Expired booking {BookingId} and restored availability", booking.BookingId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to expire booking {BookingId}", booking.BookingId);
                    await unitOfWork.RollbackTransaction();
                }
            }
            */ // End of disabled code block
            
            // Just log that the service is running (for monitoring)
            _logger.LogDebug("BookingExpirationService scan complete - expiration disabled");
        }
    }
}

