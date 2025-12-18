using AutoMapper;
using UmarahBooking.Core.Enums;
using UmarahBooking.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Core.Services
{
    public class BookingHotelService : IBookingHotelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private const int ReservationTtlMinutes = 120; // 2 hours TTL for pending reservations

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

            // Scoped Validation: Pass bookingId if it exists to allow same city/dates in DIFFERENT bookings
            await EnsureUserCanBookInCityAsync(userId, room.HotelId, dto.BookingId);
            await EnsureNoDateConflictAsync(userId, dto.CheckInDate, dto.CheckOutDate, dto.BookingId);

            int remainingRooms = await CheckRoomAvailabilityAsync(dto, room);
            if (dto.NumberOfRooms > remainingRooms)
                throw new InvalidOperationException($"Only {remainingRooms} rooms available for selected dates");

            int numberOfNights = CalculateNumberOfNights(dto.CheckInDate, dto.CheckOutDate);
            decimal totalPrice = CalculateTotalPrice(dto.NumberOfRooms, numberOfNights, room.PricePerNight);

            // Use a transaction to ensure atomicity when reserving rooms and creating pending booking
            await _unitOfWork.BeginTransaction();
            try
            {
                Booking? booking = null;

                // 1. If BookingId provided, try to find THAT specific pending booking
                if (dto.BookingId.HasValue)
                {
                    booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId.Value);

                    // Validate ownership and status
                    if (booking == null || booking.UserId != userId)
                        throw new InvalidOperationException("Booking not found or access denied");
                    
                    if (booking.BookingStatus != BookingStatus.Pending)
                         throw new InvalidOperationException("Cannot add items to a non-pending booking");
                }
                else
                {
                    // 2. Fallback (Legacy/Safety): Check if user has ANY pending booking if no ID provided
                    // Ideally frontend should always provide ID for specific drafts, but this handles "implicit" first draft
                    /*
                       NOTE: To support MULTIPLE drafts, we should ideally require BookingId or force create new if null.
                       However, for backward compat: If null, check for latest pending. If none, create new.
                    */
                     booking = await _unitOfWork.Bookings
                        .GetAllAsQuerable()
                        .Where(b => b.UserId == userId && b.BookingStatus == BookingStatus.Pending)
                        .OrderByDescending(b => b.CreatedAt) // Get latest
                        .FirstOrDefaultAsync();
                }

                if (booking == null)
                {
                    booking = new Booking
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        BookingStatus = BookingStatus.Pending,
                        // set a short reservation TTL to allow retries before expiry
                        ReservedUntil = DateTime.UtcNow.AddMinutes(ReservationTtlMinutes)
                    };
                    await _unitOfWork.Bookings.AddAsync(booking);
                }
                else
                {
                    // refresh ReservedUntil when adding another pending item
                    booking.ReservedUntil = DateTime.UtcNow.AddMinutes(ReservationTtlMinutes);
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                }

                var bookingHotel = _mapper.Map<BookingHotel>(dto);
                // Attach navigation so EF will populate FK on SaveChanges
                bookingHotel.Booking = booking;
                bookingHotel.TotalPrice = totalPrice;

                await _unitOfWork.BookingHotels.AddAsync(bookingHotel);

                // decrement availability and update room
                room.AvailableRooms -= dto.NumberOfRooms;

                if (room.AvailableRooms <= 0)
                {
                    room.AvailableRooms = 0;
                    room.IsActive = false;
                }

                await _unitOfWork.HotelRooms.UpdateAsync(room);

                // Persist all changes for this booking operation
                await _unitOfWork.SaveChanges();

                await _unitOfWork.CommitTransaction();

                return bookingHotel;
            }
            catch
            {
                await _unitOfWork.RollbackTransaction();
                throw;
            }
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
                            && b.CheckInDate < dto.CheckOutDate
                            && b.Booking.BookingStatus != BookingStatus.Cancelled
                            && b.Booking.BookingStatus != BookingStatus.Refunded) // Exclude cancelled/refunded
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
                .Where(r => r.HotelId == hotelId && r.HotelRoomId == roomId)
                .FirstOrDefaultAsync();

            if (room == null)
                throw new InvalidOperationException("Selected room not found");

            return room;
        }

      private async Task EnsureUserCanBookInCityAsync(int userId, int hotelId, int? bookingId)
{
    // 1?? Validate requested hotel exists
    var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
    if (hotel == null)
        throw new InvalidOperationException("Hotel not found");

    var requestedCity = hotel.HotelCity;

    Booking? pendingBooking = null;

    if (bookingId.HasValue)
    {
         pendingBooking = await _unitOfWork.Bookings.GetByIdAsync(bookingId.Value);
         if (pendingBooking == null || pendingBooking.UserId != userId || pendingBooking.BookingStatus != BookingStatus.Pending)
             return; // Invalid booking context, treat as new/no-conflict (or basic not found)
    }
    else
    {
        // Fallback: Get user's latest pending booking
        pendingBooking = await _unitOfWork.Bookings
            .GetAllAsQuerable()
            .Where(b => b.UserId == userId &&
                        b.BookingStatus == BookingStatus.Pending)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // If no pending booking exists, user can add this hotel freely
    if (pendingBooking == null)
        return;

    // 3?? Check if SAME pending booking already has a hotel from this city
    var existingCityHotel = await _unitOfWork.BookingHotels
        .GetAllAsQuerable()
        .Include(bh => bh.Hotel)
        .Where(bh => bh.BookingId == pendingBooking.BookingId &&
                     bh.Hotel.HotelCity == requestedCity)
        .FirstOrDefaultAsync();

    // 4?? If found ? user is trying to add another hotel from same city ? BLOCK IT
    if (existingCityHotel != null)
    {
        throw new InvalidOperationException(
            $"This pending booking already includes a hotel in {requestedCity}. " +
            $"You can only add one {requestedCity} hotel per booking."
        );
    }
}


        /// <summary>
        /// Ensures the new booking dates don't conflict with user's existing active bookings
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newCheckIn">New booking check-in date</param>
        /// <param name="newCheckOut">New booking check-out date</param>
        /// <param name="excludeBookingId">Optional: Booking ID to exclude from conflict check (for updates)</param>
        /// <exception cref="InvalidOperationException">Thrown when date conflict is detected</exception>
        /// <summary>
        /// Ensures the new booking dates don't conflict with user's existing active bookings.
        /// REFACTOR: Now primarily checks for conflicts within the SAME booking (e.g. adding 2 Makkah hotels).
        /// Overlap across DIFFERENT bookings is ALLOWED.
        /// </summary>
        private async Task EnsureNoDateConflictAsync(
            int userId,
            DateTime newCheckIn,
            DateTime newCheckOut,
            int? currentBookingId = null)
        {
            // Validate date range first
            if (newCheckIn >= newCheckOut)
            {
                throw new InvalidOperationException("Check-in date must be before check-out date");
            }

            // If we don't have a booking ID, we can't check internal consistency, return.
            // (Or we could check against "latest pending", but let's stick to explicit ID if possible)
            if (!currentBookingId.HasValue) 
                return;

            // Check for date overlap ONLY within the SAME pending booking
            // (e.g. preventing user from adding Hotel A (1-5) and Hotel B (3-7) in same trip if they are somehow allowed 2 hotels)
            // But wait, the rule is "One Makkah, One Madinah". They can overlap if they are in different cities? 
            // Usually Umrah/Hajj involves sequential stays.
            // Let's assume sequential: You can't be in Makkah and Madinah at the same time? 
            // User requirement: "Users CANNOT book overlapping dates within the SAME pending booking"
            
            var sameBookingHotels = await _unitOfWork.BookingHotels
                .GetAllAsQuerable()
                .Where(bh => bh.BookingId == currentBookingId.Value)
                .ToListAsync();

            var conflictingHotel = sameBookingHotels
                .FirstOrDefault(bh => bh.CheckInDate < newCheckOut && bh.CheckOutDate > newCheckIn);

             if (conflictingHotel != null)
            {
                // If it's the SAME city, it's already caught by EnsureUserCanBookInCityAsync (1 hotel per city).
                // If it's DIFFERENT city (e.g. Makkah vs Madinah), they MUST NOT overlap for a logical trip flow.
                
                throw new InvalidOperationException(
                    $"Date conflict within this booking! You already have a hotel reserved from {conflictingHotel.CheckInDate:yyyy-MM-dd} to {conflictingHotel.CheckOutDate:yyyy-MM-dd}. " +
                    "Please choose non-overlapping dates for your Makkah and Madinah stays."
                );
            }
        }


        async Task<IEnumerable<PendingHotelBookingDto>> IBookingHotelService.GetPendingHotelBookingsAsync(int userId)
        {
            var pendingBookings = await _unitOfWork.BookingHotels
                            .GetAllAsQuerable()
                            .Include(bh => bh.Hotel)
                            .Include(bh => bh.Room)
                            .Include(bh => bh.Booking)
                            .Where(bh => bh.Booking.UserId == userId &&
                                         bh.Booking.BookingStatus == BookingStatus.Pending &&
                                         bh.Booking.PaymentMethod == null) // Exclude if payment was attempted/selected
                            .Select(bh => new PendingHotelBookingDto
                            {
                                BookingId = bh.BookingId,
                                BookingHotelId = bh.BookingHotelId,
                                HotelId = bh.HotelId,
                                HotelName = bh.Hotel.Name,
                                City = bh.Hotel.HotelCity.ToString(),
                                RoomId = bh.RoomId,
                                RoomType = bh.Room.RoomType.ToString(),
                                NumberOfRooms = bh.NumberOfRooms,
                                CheckInDate = bh.CheckInDate,
                                CheckOutDate = bh.CheckOutDate,
                                TotalPrice = bh.TotalPrice
                            })
                            .ToListAsync();

            return pendingBookings;
        }

        public async Task<bool> DeletePendingHotelBookingAsync(int bookingHotelId, int userId)
        {
            var bookingHotel = await _unitOfWork.BookingHotels.GetByIdAsync(bookingHotelId);

            if (bookingHotel == null)
                return false;

            // Ensure the booking belongs to the user's pending booking
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingHotel.BookingId);
            if (booking == null || booking.UserId != userId || booking.BookingStatus != BookingStatus.Pending)
                return false;

            // Restore room availability if possible
            try
            {
                var room = await _unitOfWork.HotelRooms.GetByIdAsync(bookingHotel.RoomId);
                if (room != null)
                {
                    room.AvailableRooms += bookingHotel.NumberOfRooms;
                    room.IsActive = room.AvailableRooms > 0;
                    await _unitOfWork.HotelRooms.UpdateAsync(room);
                }

                await _unitOfWork.BookingHotels.DeleteAsync(bookingHotel);

                // If booking has no more related booking items, remove booking as well
                var remainingItems = await _unitOfWork.BookingHotels
                    .GetAllAsQuerable()
                    .CountAsync(bh => bh.BookingId == booking.BookingId);

                if (remainingItems == 0)
                {
                    await _unitOfWork.Bookings.DeleteAsync(booking);
                }

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

