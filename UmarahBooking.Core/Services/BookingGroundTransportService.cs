using AutoMapper;
using Manisik.Enums;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    public class BookingGroundTransportService : IBookingGroundTransportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingGroundTransportService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Book ground transport and automatically create or reuse a Booking for the user
        /// </summary>
        public async Task<BookingGroundTransport> BookGroundTransportAsync(GroundTransportBookingDto dto, int userId)
        {
            // Validate service date
            ValidateServiceDate(dto.ServiceDate);

            // Get ground transport
            var groundTransport = await GetGroundTransportAsync(dto.GroundTransportId);

            // Check capacity
            if (dto.NumberOfPassengers > groundTransport.Capacity)
            {
                throw new InvalidOperationException(
                    $"Requested passengers ({dto.NumberOfPassengers}) exceeds transport capacity ({groundTransport.Capacity})");
            }

            // Calculate total price
            decimal totalPrice = CalculateTotalPrice(dto.NumberOfPassengers, groundTransport.PricePerPerson);

            // Get or create pending booking
            var booking = await _unitOfWork.Context.Set<Booking>()
                .Where(b => b.UserId == userId && b.BookingStatus == BookingStatus.Pending)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                booking = new Booking
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    BookingStatus = BookingStatus.Pending,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = 0,
                    ServiceFee = 0
                };
                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChanges();  // ✅ Save booking first

            }

            // Create booking ground transport
            // Create booking ground transport - Manual mapping instead of AutoMapper
            var bookingGroundTransport = new BookingGroundTransport
            {
                GroundTransportId = dto.GroundTransportId,
                ServiceDate = dto.ServiceDate,
                PickupLocation = dto.PickupLocation,
                DropoffLocation = dto.DropoffLocation,
                NumberOfPassengers = dto.NumberOfPassengers,
                TotalPrice = totalPrice,
                Booking = booking
            };


            await _unitOfWork.BookingGroundTransports.AddAsync(bookingGroundTransport);

            // Persist changes
            await _unitOfWork.SaveChanges();

            return bookingGroundTransport;
        }

        public void ValidateServiceDate(DateTime serviceDate)
        {
            if (serviceDate.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Service date must be in the future");
            }
        }

        public decimal CalculateTotalPrice(int passengers, decimal pricePerPerson)
        {
            return passengers * pricePerPerson;
        }

        public async Task<GroundTransport> GetGroundTransportAsync(int transportId)
        {
            var transport = await _unitOfWork.GroundTransports.GetByIdAsync(transportId);

            if (transport == null)
            {
                throw new InvalidOperationException("Selected ground transport not found");
            }

            if (!transport.IsActive)
            {
                throw new InvalidOperationException("Selected ground transport is not available");
            }

            return transport;
        }

        public async Task<IEnumerable<PendingGroundBookingDto>> GetPendingGroundBookingsAsync(int userId)
        {
            var pendingBookings = await _unitOfWork.BookingGroundTransports
                .GetAllAsQuerable()
                .Include(bgt => bgt.GroundTransport)
                .Include(bgt => bgt.Booking)
                .Where(bgt => bgt.Booking.UserId == userId &&
                              bgt.Booking.BookingStatus == BookingStatus.Pending &&
                              bgt.Booking.PaymentMethod == null)
                .Select(bgt => new PendingGroundBookingDto
                {
                    BookingId = bgt.BookingId,
                    BookingGroundTransportId = bgt.BookingGroundTransportId,
                    GroundTransportId = bgt.GroundTransportId,
                    ServiceName = bgt.GroundTransport.ServiceName,
                    PickupLocation = bgt.PickupLocation,
                    DropoffLocation = bgt.DropoffLocation,
                    ServiceDate = bgt.ServiceDate,
                    NumberOfPassengers = bgt.NumberOfPassengers,
                    TotalPrice = bgt.TotalPrice
                })
                .ToListAsync();

            return pendingBookings;
        }

        public async Task<bool> DeletePendingGroundBookingAsync(int bookingGroundTransportId, int userId)
        {
            var bookingGround = await _unitOfWork.BookingGroundTransports.GetByIdAsync(bookingGroundTransportId);

            if (bookingGround == null)
                return false;

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingGround.BookingId);
            if (booking == null || booking.UserId != userId || booking.BookingStatus != BookingStatus.Pending)
                return false;

            try
            {
                await _unitOfWork.BookingGroundTransports.DeleteAsync(bookingGround);
                await _unitOfWork.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
