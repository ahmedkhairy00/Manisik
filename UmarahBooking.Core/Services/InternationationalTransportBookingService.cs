using AutoMapper;
using Manisik.Enums;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    public class InternationationalTransportBookingService : IInternationalTransportBookingService

    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public InternationationalTransportBookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BookingInternationalTransport> BookInternationalTransportAsync(int userId, TransportBookingDto dto)
        {
            // Check flight exists
            var flight = await _unitOfWork.InternationalTransports.GetByIdAsync(dto.TransportId);
            if (flight == null) throw new InvalidOperationException("Flight not found");

            // Check seats availability
            if (dto.NumberOfSeats > flight.AvailableSeats)
                throw new InvalidOperationException($"Only {flight.AvailableSeats} seats are available for this flight.");





            // Get or create Booking
            var booking = await _unitOfWork.Bookings
                .GetAllAsQuerable()
                .Where(b => b.UserId == userId && b.BookingStatus == BookingStatus.Pending)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                booking = new Booking
                {
                    UserId = userId,
                    BookingStatus = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Bookings.AddAsync(booking);
            }
            var existingFlightsCount = await _unitOfWork.BookingInternationalTransports
                .GetAllAsQuerable()
                .Where(b => b.BookingId == booking.BookingId)
                .CountAsync();

            if (existingFlightsCount >= 2)
            {
                throw new InvalidOperationException("You can only book 1 flight");
            }
            // Map DTO to Entity
            var bookingTransport = _mapper.Map<BookingInternationalTransport>(dto);
            bookingTransport.BookingId = booking.BookingId;

            await _unitOfWork.BookingInternationalTransports.AddAsync(bookingTransport);

            // Update flight seats
            flight.AvailableSeats -= dto.NumberOfSeats;
            if (flight.AvailableSeats < 0) flight.AvailableSeats = 0;
            await _unitOfWork.InternationalTransports.UpdateAsync(flight);



            return bookingTransport;
        }

    }
}
