using AutoMapper;
using Manisik.Enums;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    public class BookingInternationalTransportService : IBookingInternationalTransportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingInternationalTransportService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Book international transport and automatically create or reuse a Booking for the user
        /// </summary>
        public async Task<BookingInternationalTransport> BookInternationalTransportAsync(InternationalTransportBookingDto dto, int userId)
        {
            // Validate departure date
            if (dto.DepartureDate.HasValue)
            {
                ValidateDepartureDate(dto.DepartureDate.Value);
            }

            // Get international transport
            var internationalTransport = await GetInternationalTransportAsync((int)dto.TransportId);

            // Check seat availability
            int availableSeats = await CheckSeatAvailabilityAsync((int)dto.TransportId, dto.NumberOfSeats);
            if (dto.NumberOfSeats > availableSeats)
            {
                throw new InvalidOperationException(
                    $"Only {availableSeats} seats available for this transport");
            }

            // Calculate total price
            decimal totalPrice = CalculateTotalPrice(dto.NumberOfSeats, internationalTransport.Price);

            // Get or create pending booking
            // ✅ Use Context directly with tracking
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
                };
                await _unitOfWork.Bookings.AddAsync(booking);

                // 👇 ADD THIS: Save the booking first to generate its ID
                await _unitOfWork.SaveChanges();
            }

            // Create booking international transport
            // With this:
            var bookingInternationalTransport = new BookingInternationalTransport
            {
                InternationalTransportId = dto.TransportId.Value,
                NumberOfSeats = dto.NumberOfSeats,
                TotalPrice = totalPrice,
                Booking = booking
            };          
            // Attach the Booking navigation so EF will correctly set the FK when saving

            await _unitOfWork.BookingInternationalTransports.AddAsync(bookingInternationalTransport);

            // Persist changes
            await _unitOfWork.SaveChanges();

            return bookingInternationalTransport;
        }

        public void ValidateDepartureDate(DateTime departureDate)
        {
            if (departureDate.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Departure date must be in the future");
            }
        }

        public decimal CalculateTotalPrice(int seats, decimal pricePerSeat)
        {
            return seats * pricePerSeat;
        }

        public async Task<InternationalTransport> GetInternationalTransportAsync(int transportId)
        {
            var transport = await _unitOfWork.InternationalTransports.GetByIdAsync(transportId);

            if (transport == null)
            {
                throw new InvalidOperationException("Selected international transport not found");
            }

            if (!transport.IsActive)
            {
                throw new InvalidOperationException("Selected international transport is not available");
            }

            return transport;
        }

        public async Task<int> CheckSeatAvailabilityAsync(int transportId, int requestedSeats)
        {
            var transport = await GetInternationalTransportAsync(transportId);

            // Get existing bookings for this transport
            var existingBookings = await _unitOfWork.BookingInternationalTransports
                .GetAllAsQuerable()
                .Where(b => b.InternationalTransportId == transportId)
                .ToListAsync();

            int bookedSeats = existingBookings.Sum(b => b.NumberOfSeats);
            int availableSeats = transport.AvailableSeats - bookedSeats;

            return availableSeats;
        }

        public async Task<IEnumerable<PendingTransportBookingDto>> GetPendingTransportBookingsAsync(int userId)
        {
            var pendingBookings = await _unitOfWork.BookingInternationalTransports
                .GetAllAsQuerable()
                .Include(bit => bit.InternationalTransport)
                .Include(bit => bit.Booking)
                .Where(bit => bit.Booking.UserId == userId &&
                              bit.Booking.BookingStatus == BookingStatus.Pending &&
                              bit.Booking.PaymentMethod == null)
                .Select(bit => new PendingTransportBookingDto
                {
                    BookingId = bit.BookingId,
                    BookingInternationalTransportId = bit.BookingInternationalTransportId,
                    InternationalTransportId = bit.InternationalTransportId,
                    TransportId = bit.InternationalTransportId,
                    TransportType = bit.InternationalTransport.TransportType.ToString(),
                    CarrierName = bit.InternationalTransport.CarrierName,
                    FlightNumber = bit.InternationalTransport.FlightNumber,
                    DepartureDate = bit.InternationalTransport.DepartureDate,
                    DepartureAirport = bit.InternationalTransport.DepartureAirport.ToString(),
                    ArrivalAirport = bit.InternationalTransport.ArrivalAirport.ToString(),
                    NumberOfSeats = bit.NumberOfSeats,
                    TotalPrice = bit.TotalPrice
                })
                .ToListAsync();

            return pendingBookings;
        }

        public async Task<bool> DeletePendingInternationalBookingAsync(int bookingInternationalTransportId, int userId)
        {
            var bookingInternational = await _unitOfWork.BookingInternationalTransports.GetByIdAsync(bookingInternationalTransportId);

            if (bookingInternational == null)
                return false;

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingInternational.BookingId);
            if (booking == null || booking.UserId != userId || booking.BookingStatus != BookingStatus.Pending)
                return false;

            try
            {
                // No seat restore required because seats are derived from transport.available - bookings sum
                await _unitOfWork.BookingInternationalTransports.DeleteAsync(bookingInternational);
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
