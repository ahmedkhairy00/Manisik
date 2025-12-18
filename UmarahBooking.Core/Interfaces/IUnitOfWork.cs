using UmarahBooking.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        // ========== REPOSITORY PROPERTIES ==========

        // ? Add this property
        DbContext Context { get; }

        /// <summary>
        /// Repository for Hotel entity operations
        /// </summary>
        IBaseRepository<Hotel> Hotels { get; }

        /// <summary>
        /// Repository for HotelRoom entity operations
        /// </summary>
        IBaseRepository<HotelRoom> HotelRooms { get; }

        /// <summary>
        /// Repository for InternationalTransport entity operations
        /// </summary>
        IBaseRepository<InternationalTransport> InternationalTransports { get; }

        /// <summary>
        /// Repository for GroundTransport entity operations
        /// </summary>
        IBaseRepository<GroundTransport> GroundTransports { get; }

        /// <summary>
        /// Repository for Booking entity operations
        /// </summary>
        IBaseRepository<Booking> Bookings { get; }

        /// <summary>
        /// Repository for BookingHotel entity operations
        /// </summary>
        IBaseRepository<BookingHotel> BookingHotels { get; }

        /// <summary>
        /// Repository for BookingInternationalTransport entity operations
        /// </summary>
        IBaseRepository<BookingInternationalTransport> BookingInternationalTransports { get; }

        /// <summary>
        /// Repository for BookingGroundTransport entity operations
        /// </summary>
        IBaseRepository<BookingGroundTransport> BookingGroundTransports { get; }

        /// <summary>
        /// Repository for Traveler entity operations
        /// </summary>
        IBaseRepository<Traveler> Travelers { get; }

        /// <summary>
        /// Repository for Payment entity operations
        /// </summary>
        IBaseRepository<Payment> Payments { get; }

        /// <summary>
        /// Repository for PaymentEvent entity operations
        /// </summary>
        IBaseRepository<PaymentEvent> PaymentEvents { get; }

        /// <summary>
        /// Repository for ApplicationUser entity operations
        /// </summary>
        IBaseRepository<ApplicationUser> Users { get; }

        /// <summary>
        /// Repository for Subscriber entity operations
        /// </summary>
        IBaseRepository<Subscriber> Subscribers { get; }

        // ========== TRANSACTION METHODS ==========

        /// <summary>
        /// Saves all changes made in this unit of work to the database
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        Task<int> SaveChanges();

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        Task BeginTransaction();

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        Task CommitTransaction();

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        Task RollbackTransaction();
    }
}

