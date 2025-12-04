using AutoMapper;
using Manisik.Enums;
using Manisik.Models;
using Microsoft.EntityFrameworkCore;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    public class BookingHotelService : IBookingHotelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingHotelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Book hotel and automatically create or reuse a Booking for the user
        /// </summary>
        public async Task<BookingHotel> BookHotelAsync(HotelBookingDto dto, int userId)
        {
            ValidateDates(dto);

            var room = await GetRoomAsync(dto.HotelId, dto.RoomId);

            await EnsureUserCanBookInCityAsync(userId, room.HotelId);
            await EnsureNoDateConflictAsync(userId, dto.CheckInDate, dto.CheckOutDate);

            int remainingRooms = await CheckRoomAvailabilityAsync(dto, room);
            if (dto.NumberOfRooms > remainingRooms)
                throw new InvalidOperationException($"Only {remainingRooms} rooms available for selected dates");

            int numberOfNights = CalculateNumberOfNights(dto.CheckInDate, dto.CheckOutDate);
            decimal totalPrice = CalculateTotalPrice(dto.NumberOfRooms, numberOfNights, room.PricePerNight);


            var booking = await _unitOfWork.Bookings
                .GetAllAsQuerable()
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
            }

            var bookingHotel = _mapper.Map<BookingHotel>(dto);
            bookingHotel.BookingId = booking.BookingId;
            bookingHotel.TotalPrice = totalPrice;

            await _unitOfWork.BookingHotels.AddAsync(bookingHotel);

            room.AvailableRooms -= dto.NumberOfRooms;

            if (room.AvailableRooms <= 0)
            {
                room.AvailableRooms = 0;
                room.IsActive = false;
            }

            await _unitOfWork.HotelRooms.UpdateAsync(room);

            return bookingHotel;
        }

        public int CalculateNumberOfNights(DateTime checkIn, DateTime checkOut)
        {
            return (int)(checkOut.Date - checkIn.Date).TotalDays;
        }

        public decimal CalculateTotalPrice(int numberOfRooms, int numberOfNights, decimal pricePerNight)
        {
            return numberOfRooms * numberOfNights * pricePerNight;
        }

        public async Task<int> CheckRoomAvailabilityAsync(HotelBookingDto dto, HotelRoom room)
        {
            var overlappingBookings = await _unitOfWork.BookingHotels
                .GetAllAsQuerable()
                .Where(b => b.HotelId == dto.HotelId
                            && b.RoomId == dto.RoomId
                            && b.CheckOutDate > dto.CheckInDate
                            && b.CheckInDate < dto.CheckOutDate)
                .ToListAsync();

            int bookedRooms = overlappingBookings.Sum(b => b.NumberOfRooms);
            return room.AvailableRooms - bookedRooms;
        }

        public void ValidateDates(HotelBookingDto dto)
        {
            if (dto.CheckInDate >= dto.CheckOutDate)
                throw new InvalidOperationException("Check-in date must be before check-out date");
        }

        public async Task<HotelRoom> GetRoomAsync(int hotelId, int roomId)
        {
            var room = await _unitOfWork.HotelRooms
                .GetAllAsQuerable()
                .Where(r => r.HotelId == hotelId && r.HotelRoomId == roomId && r.IsActive)
                .FirstOrDefaultAsync();

            if (room == null)
                throw new InvalidOperationException("Selected room not found");

            return room;
        }
        private async Task EnsureUserCanBookInCityAsync(int userId, int hotelId)
        {

            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            if (hotel == null)
                throw new InvalidOperationException("Hotel not found");

            string city = hotel.HotelCity.ToString().Trim().ToLower() ?? "";


            var existingBooking = await _unitOfWork.BookingHotels
                .GetAllAsQuerable()
                .Include(bh => bh.Hotel)
                .Where(bh => bh.Booking.UserId == userId &&
                             bh.Hotel.HotelCity.ToString().ToLower() == city)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                throw new InvalidOperationException(
                    $"You already booked a hotel in {hotel.HotelCity.ToString()} , You can only book once per city"
                );
            }
        }
        private async Task EnsureNoDateConflictAsync(int userId, DateTime newCheckIn, DateTime newCheckOut)
        {
            var existingBookings = await _unitOfWork.BookingHotels
                .GetAllAsQuerable()
                .Include(bh => bh.Hotel)
                .Where(bh => bh.Booking.UserId == userId)
                .ToListAsync();

            foreach (var booking in existingBookings)
            {
                if (booking.CheckOutDate > newCheckIn && booking.CheckInDate < newCheckOut)
                {
                    throw new InvalidOperationException(
                        "Your new booking dates conflict with existing reservations"
                    );
                }
            }
        }


    }
}
