using Manisik.DTOs;
using Manisik.Models;
using Manisik.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Manisik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IUmrahBookingRepository _bookingRepo;

        public ReservationController(IUmrahBookingRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        // ✅ Reserve Hotel
        [HttpPost("hotel")]
        public async Task<IActionResult> ReserveHotel([FromBody] ReserveHotelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "❌ Invalid reservation data" });

            var booking = new UmrahBooking
            {
                TripType = dto.TripType,
                FullName = dto.FullName,
                NationalId = dto.NationalId,
                Email = dto.Email,
                Phone = dto.Phone,
                TravelMode = dto.TravelMode,
                DepartureAirport = dto.DepartureAirport,
                Airline = dto.Airline,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DepartureDate = dto.DepartureDate,
                ArrivalDate = dto.ArrivalDate,
                TravelPrice = dto.TravelPrice,
                AuthId = dto.AuthId
            };

            var created = await _bookingRepo.AddBookingAsync(booking);
            if (created == null)
                return BadRequest(new { message = "❌ Failed to create booking" });

            var hotelBooking = await _bookingRepo.AddHotelToBookingAsync(
                created.UmrahBookingId, dto.HotelId, dto.CheckIn, dto.CheckOut);

            if (hotelBooking == null)
                return BadRequest(new { message = "❌ Failed to add hotel to booking" });

            return Ok(new
            {
                message = "✅ Hotel reservation created successfully",
                bookingId = created.UmrahBookingId,
                hotelBookingId = hotelBooking.UmrahBookingHotelId
            });
        }

        // ✅ Reserve Transport
        [HttpPost("transport")]
        public async Task<IActionResult> ReserveTransport([FromBody] ReserveTransportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "❌ Invalid reservation data" });

            var booking = new UmrahBooking
            {
                TripType = dto.TripType,
                FullName = dto.FullName,
                NationalId = dto.NationalId,
                Email = dto.Email,
                Phone = dto.Phone,
                TravelMode = dto.TravelMode,
                DepartureAirport = dto.DepartureAirport,
                Airline = dto.Airline,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DepartureDate = dto.DepartureDate,
                ArrivalDate = dto.ArrivalDate,
                TravelPrice = dto.TravelPrice,
                AuthId = dto.AuthId,
                TransportId = dto.TransportId
            };

            var created = await _bookingRepo.AddBookingAsync(booking);
            if (created == null)
                return BadRequest(new { message = "❌ Failed to create booking" });

            return Ok(new
            {
                message = "✅ Transport reservation created successfully",
                bookingId = created.UmrahBookingId
            });
        }
    }
}
