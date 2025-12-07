using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Data.Repositories
{
    /// <summary>
    /// Implementation of Unit of Work pattern
    /// Coordinates work of multiple repositories and maintains a single DbContext
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        #region Private Fields

        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repository instances (lazy initialization)

        private IBaseRepository<Hotel>? _hotels;
        private IBaseRepository<HotelRoom>? _hotelRooms;
        private IBaseRepository<InternationalTransport>? _internationalTransports;
        private IBaseRepository<GroundTransport>? _groundTransports;
        private IBaseRepository<Booking>? _bookings;
        private IBaseRepository<BookingHotel>? _bookingHotels;
        private IBaseRepository<BookingInternationalTransport>? _bookingInternationalTransports;
        private IBaseRepository<BookingGroundTransport>? _bookingGroundTransports;
        private IBaseRepository<Traveler>? _travelers;
        private IBaseRepository<Payment>? _payments;
        private IBaseRepository<ApplicationUser>? _users;
        private IBaseRepository<Subscriber>? _subscribers;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of UnitOfWork
        /// </summary>
        /// <param name="context">Database context</param>
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region Repository Properties (Lazy Initialization)

        /// <summary>
        /// Gets the Hotels repository
        /// Creates a new instance if one doesn't exist (lazy initialization)
        /// </summary>
        /// 
        public DbContext Context => _context;

        public IBaseRepository<Hotel> Hotels
        {
            get
            {
                _hotels ??= new BaseRepository<Hotel>(_context);
                return _hotels;
            }
        }

        /// <summary>
        /// Gets the HotelRooms repository
        /// </summary>
        public IBaseRepository<HotelRoom> HotelRooms
        {
            get
            {
                _hotelRooms ??= new BaseRepository<HotelRoom>(_context);
                return _hotelRooms;
            }
        }

        /// <summary>
        /// Gets the InternationalTransports repository
        /// </summary>
        public IBaseRepository<InternationalTransport> InternationalTransports
        {
            get
            {
                _internationalTransports ??= new BaseRepository<InternationalTransport>(_context);
                return _internationalTransports;
            }
        }

        /// <summary>
        /// Gets the GroundTransports repository
        /// </summary>
        public IBaseRepository<GroundTransport> GroundTransports
        {
            get
            {
                _groundTransports ??= new BaseRepository<GroundTransport>(_context);
                return _groundTransports;
            }
        }

        /// <summary>
        /// Gets the Bookings repository
        /// </summary>
        public IBaseRepository<Booking> Bookings
        {
            get
            {
                _bookings ??= new BaseRepository<Booking>(_context);
                return _bookings;
            }
        }

        /// <summary>
        /// Gets the BookingHotels repository
        /// </summary>
        public IBaseRepository<BookingHotel> BookingHotels
        {
            get
            {
                _bookingHotels ??= new BaseRepository<BookingHotel>(_context);
                return _bookingHotels;
            }
        }

        /// <summary>
        /// Gets the BookingInternationalTransports repository
        /// </summary>
        public IBaseRepository<BookingInternationalTransport> BookingInternationalTransports
        {
            get
            {
                _bookingInternationalTransports ??= new BaseRepository<BookingInternationalTransport>(_context);
                return _bookingInternationalTransports;
            }
        }

        /// <summary>
        /// Gets the BookingGroundTransports repository
        /// </summary>
        public IBaseRepository<BookingGroundTransport> BookingGroundTransports
        {
            get
            {
                _bookingGroundTransports ??= new BaseRepository<BookingGroundTransport>(_context);
                return _bookingGroundTransports;
            }
        }

        /// <summary>
        /// Gets the Travelers repository
        /// </summary>
        public IBaseRepository<Traveler> Travelers
        {
            get
            {
                _travelers ??= new BaseRepository<Traveler>(_context);
                return _travelers;
            }
        }

        /// <summary>
        /// Gets the Payments repository
        /// </summary>
        public IBaseRepository<Payment> Payments
        {
            get
            {
                _payments ??= new BaseRepository<Payment>(_context);
                return _payments;
            }
        }

        /// <summary>
        /// Gets the Users repository
        /// </summary>
        public IBaseRepository<ApplicationUser> Users
        {
            get
            {
                _users ??= new BaseRepository<ApplicationUser>(_context);
                return _users;
            }
        }

        /// <summary>
        /// Gets the Subscribers repository
        /// </summary>
        public IBaseRepository<Subscriber> Subscribers
        {
            get
            {
                _subscribers ??= new BaseRepository<Subscriber>(_context);
                return _subscribers;
            }
        }

        #endregion

        #region Transaction Methods

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public async Task<int> SaveChanges()
        {
            // Let EF throw its original exceptions so callers can react to specific exception types
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Begins a new database transaction
        /// Useful when you need to ensure multiple operations succeed or fail together
        /// </summary>
        public async Task BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commits the current transaction
        /// Permanently saves all changes made during the transaction
        /// </summary>
        public async Task CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransaction();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rolls back the current transaction
        /// Discards all changes made during the transaction
        /// </summary>
        public async Task RollbackTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// Releases all resources used by the UnitOfWork
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _transaction?.Dispose();
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer (destructor)
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }

        #endregion
    }
}
