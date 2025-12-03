using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Controllers
{
    /// <summary>
    /// Controller for managing hotel operations including CRUD, search, and booking
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        #region Dependencies

        private readonly ILogger<HotelController> _logger;

        private readonly IHotelService _hotelService;

        //private readonly IUnitOfWork _unitOfWork;
        //private readonly IMapper _mapper;

        public HotelController(
            ILogger<HotelController> logger,
            IHotelService hotelService
            //IUnitOfWork unitOfWork,
            //IMapper mapper, 
            )
        {
            _logger = logger;

            _hotelService = hotelService;
            //_unitOfWork = unitOfWork;
            //_mapper = mapper;
        }

        #endregion

        #region GET Operations


        [HttpGet("getallFiltered")]
        public async Task<IActionResult> GetAllFiltered([FromQuery] string? city, [FromQuery] string? filter)
        {
            try
            {
                var hotels = await _hotelService.GetFilteredHotelsAsync(city, filter);
                if (hotels == null)
                {
                    _logger.LogWarning("No hotels found");
                    return NotFound(ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
                        "No hotels found"));
                }

                return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResponse(
                    hotels, "Hotels retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving hotels");
                return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
                    "An error occurred while retrieving the hotels"));
            }
        }


        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("GetHotelById/{id:int}")]
        public async Task<IActionResult> GetHotelById(int id)
        {
            try
            {
                var hotelDto = await _hotelService.GetHotelByIdAsync(id);

                if (hotelDto == null)
                {
                    _logger.LogWarning("Hotel with ID {HotelId} not found", id);
                    return NotFound(ApiResponse<HotelDto>.ErrorResponse(
                        $"Hotel with ID {id} not found"));
                }

                return Ok(ApiResponse<HotelDto>.SuccessResponse(
                    hotelDto, "Hotel retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving hotel with ID {HotelId}", id);
                return StatusCode(500, ApiResponse<HotelDto>.ErrorResponse(
                    "An error occurred while retrieving the hotel"));
            }
        }

        //[HttpGet("GetAllHotels")]
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(ApiResponse<IEnumerable<HotelDto>>), 200)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> GetAllHotels()
        //{
        //    try
        //    {
        //        var hotels = await _unitOfWork.Hotels.GetAllAsync();
        //        var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);
        //        return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResponse(
        //            hotelDtos, "Hotels retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while retrieving all hotels");
        //        return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //            "An error occurred while retrieving hotels"));
        //    }
        //}

        //[HttpGet("GetHotelByRating/{rating:int}")]
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(ApiResponse<HotelDto>), 200)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> GetHotelByRating(int rating)
        //{
        //    try
        //    {
        //        if (rating < 1 || rating > 5)
        //            return BadRequest(ApiResponse<HotelDto>.ErrorResponse(
        //                "Rating must be between 1 and 5"));

        //        var hotel = await _unitOfWork.Hotels.FindBySearch(h => h.StarRating == rating);
        //        if (hotel == null)
        //        {
        //            _logger.LogInformation("No hotel found with rating {Rating}", rating);
        //            return NotFound(ApiResponse<HotelDto>.ErrorResponse(
        //                $"No hotel found with {rating} star rating"));
        //        }

        //        var hotelDto = _mapper.Map<HotelDto>(hotel);
        //        return Ok(ApiResponse<HotelDto>.SuccessResponse(
        //            hotelDto, $"Hotel with {rating} stars retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while retrieving hotel with rating {Rating}", rating);
        //        return StatusCode(500, ApiResponse<HotelDto>.ErrorResponse(
        //            "An error occurred while retrieving the hotel"));
        //    }
        //}

        //[HttpGet("GetHotelsByAllRooms")]
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(ApiResponse<IEnumerable<HotelDto>>), 200)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> GetHotelsByAllRooms()
        //{
        //    try
        //    {
        //        var hotels = await _unitOfWork.Hotels.FindWithAsync(new[] { "Rooms" });
        //        if (!hotels.Any())
        //        {
        //            _logger.LogInformation("No hotels with rooms found");
        //            return NotFound(ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //                "No hotels with rooms available"));
        //        }

        //        var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);
        //        return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResponse(
        //            hotelDtos, "Hotels with rooms retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while retrieving hotels with rooms");
        //        return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //            "An error occurred while retrieving hotels"));
        //    }
        //}

        //[HttpGet("GetAllHotelsBySearch/{name}")]
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(ApiResponse<IEnumerable<HotelDto>>), 200)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> GetAllHotelsBySearch(string name)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(name))
        //            return BadRequest(ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //                "Search name cannot be empty"));

        //        var hotels = await _unitOfWork.Hotels.FindAllBySearch(h => h.Name.Contains(name));
        //        if (!hotels.Any())
        //        {
        //            _logger.LogInformation("No hotels found matching search term: {SearchTerm}", name);
        //            return NotFound(ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //                $"No hotels found matching '{name}'"));
        //        }

        //        var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);
        //        return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResponse(
        //            hotelDtos, $"{hotels.Count()} hotels found"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while searching hotels with name: {Name}", name);
        //        return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //            "An error occurred while searching hotels"));
        //    }
        //}

        //[HttpGet("hotels/search")]
        //public async Task<IActionResult> GetHotelsBySearchAndSkipWithOrder(
        //    string? search = null,
        //    int take = 10,
        //    int skip = 0,
        //    string orderBy = "Name",
        //    string orderDirection = "Asc")
        //{
        //    try
        //    {
        //        if (take <= 0 || take > 100)
        //            return BadRequest(ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //                "Take value must be between 1 and 100"));
        //        if (skip < 0)
        //            return BadRequest(ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //                "Skip value cannot be negative"));

        //        Expression<Func<Hotel, object>> orderExpression = orderBy.ToLower() switch
        //        {
        //            "name" => h => h.Name!,
        //            "distancetoharam" => h => h.DistanceToHaram!,
        //            "starrating" => h => h.StarRating,
        //            "city" => h => h.HotelCity!,
        //            _ => h => h.Name!
        //        };

        //        var sortDirection = orderDirection.ToLower() == "asc"
        //            ? OrderBy.Ascending
        //            : OrderBy.Descending;

        //        Expression<Func<Hotel, bool>> criteria = h => true;
        //        if (!string.IsNullOrEmpty(search))
        //            criteria = h => h.Name.Contains(search);

        //        var hotels = await _unitOfWork.Hotels.FindAllBySearchAndSkipWithOrder(
        //            criteria: criteria,
        //            take: take,
        //            skip: skip,
        //            orderBy: orderExpression,
        //            orderByDirection: sortDirection
        //        );

        //        var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);
        //        var message = $"Retrieved {hotels.Count()} hotels";
        //        if (!string.IsNullOrEmpty(search))
        //            message += $" matching '{search}'";
        //        message += $" (Page {(skip / take) + 1})";

        //        return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResponse(hotelDtos, message));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred during advanced hotel search");
        //        return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResponse(
        //            "An error occurred while searching hotels"));
        //    }
        //}

        //#endregion

        //#region POST Operations

        //[HttpPost("CreateHotel")]
        //[Authorize(Roles = "Admin,HotelManager")]
        //[ProducesResponseType(typeof(ApiResponse<HotelDto>), 201)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(403)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> CreateHotel(HotelDto hotelDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //            return BadRequest(ApiResponse<HotelDto>.ErrorResponse("Validation failed", errors));
        //        }

        //        if (hotelDto.StarRating < 1 || hotelDto.StarRating > 5)
        //            return BadRequest(ApiResponse<HotelDto>.ErrorResponse("Star rating must be between 1 and 5"));

        //        var hotel = _mapper.Map<Hotel>(hotelDto);
        //        hotel.CreatedAt = DateTime.UtcNow;
        //        hotel.IsActive = true;

        //        await _unitOfWork.Hotels.AddAsync(hotel);
        //        await _unitOfWork.SaveChanges();

        //        var createdHotelDto = _mapper.Map<HotelDto>(hotel);
        //        return CreatedAtAction(nameof(GetHotelById), new { id = hotel.HotelId },
        //            ApiResponse<HotelDto>.SuccessResponse(createdHotelDto, "Hotel created successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while creating hotel");
        //        return StatusCode(500, ApiResponse<HotelDto>.ErrorResponse(
        //            "An error occurred while creating the hotel"));
        //    }
        //}

        //[HttpPost("BookHotel")]
        //[Authorize(Roles = "User,Admin")]
        //[ProducesResponseType(typeof(ApiResponse<BookingDto>), 201)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> BookHotel(BookingDto bookingDto)
        //{
        //    try
        //    {
        //        if (bookingDto == null)
        //            return BadRequest(ApiResponse<BookingDto>.ErrorResponse("Booking data is required"));

        //        if (bookingDto.MakkahHotel == null && bookingDto.MadinahHotel == null)
        //            return BadRequest(ApiResponse<BookingDto>.ErrorResponse(
        //                "At least one hotel (Makkah or Madinah) must be selected"));

        //        var booking = _mapper.Map<Booking>(bookingDto);
        //        booking.BookingDate = DateTime.UtcNow;
        //        booking.CreatedAt = DateTime.UtcNow;

        //        await _unitOfWork.Bookings.AddAsync(booking);
        //        await _unitOfWork.SaveChanges();

        //        var bookingHotels = new List<BookingHotel>();

        //        // Makkah Hotel
        //        if (bookingDto.MakkahHotel != null)
        //        {
        //            var makkahHotel = await _unitOfWork.Hotels.GetByIdAsync(bookingDto.MakkahHotel.HotelId);
        //            if (makkahHotel == null)
        //                return NotFound(ApiResponse<BookingDto>.ErrorResponse(
        //                    $"Makkah hotel with ID {bookingDto.MakkahHotel.HotelId} not found"));

        //            var makkahRoom = await _unitOfWork.HotelRooms.GetByIdAsync(bookingDto.MakkahHotel.RoomId);
        //            if (makkahRoom == null || makkahRoom.AvailableRooms < bookingDto.MakkahHotel.NumberOfRooms)
        //                return BadRequest(ApiResponse<BookingDto>.ErrorResponse(
        //                    "Insufficient room availability for Makkah hotel"));

        //            bookingHotels.Add(new BookingHotel
        //            {
        //                BookingId = booking.BookingId,
        //                HotelId = makkahHotel.HotelId,
        //                RoomId = bookingDto.MakkahHotel.RoomId,
        //                City = Manisik.Enums.HotelCity.Makkah,
        //                CheckInDate = bookingDto.MakkahHotel.CheckInDate,
        //                CheckOutDate = bookingDto.MakkahHotel.CheckOutDate,
        //                NumberOfRooms = bookingDto.MakkahHotel.NumberOfRooms,
        //                TotalPrice = bookingDto.MakkahHotel.TotalPrice ?? 0m
        //            });
        //        }

        //        // Madinah Hotel
        //        if (bookingDto.MadinahHotel != null)
        //        {
        //            var madinahHotel = await _unitOfWork.Hotels.GetByIdAsync(bookingDto.MadinahHotel.HotelId);
        //            if (madinahHotel == null)
        //                return NotFound(ApiResponse<BookingDto>.ErrorResponse(
        //                    $"Madinah hotel with ID {bookingDto.MadinahHotel.HotelId} not found"));

        //            var madinahRoom = await _unitOfWork.HotelRooms.GetByIdAsync(bookingDto.MadinahHotel.RoomId);
        //            if (madinahRoom == null || madinahRoom.AvailableRooms < bookingDto.MadinahHotel.NumberOfRooms)
        //                return BadRequest(ApiResponse<BookingDto>.ErrorResponse(
        //                    "Insufficient room availability for Madinah hotel"));

        //            bookingHotels.Add(new BookingHotel
        //            {
        //                BookingId = booking.BookingId,
        //                HotelId = madinahHotel.HotelId,
        //                RoomId = bookingDto.MadinahHotel.RoomId,
        //                City = Manisik.Enums.HotelCity.Madinah,
        //                CheckInDate = bookingDto.MadinahHotel.CheckInDate,
        //                CheckOutDate = bookingDto.MadinahHotel.CheckOutDate,
        //                NumberOfRooms = bookingDto.MadinahHotel.NumberOfRooms,
        //                TotalPrice = bookingDto.MadinahHotel.TotalPrice ?? 0m
        //            });
        //        }

        //        foreach (var bh in bookingHotels)
        //            await _unitOfWork.BookingHotels.AddAsync(bh);

        //        await _unitOfWork.SaveChanges();

        //        return CreatedAtAction(nameof(GetHotelById), new { id = booking.BookingId },
        //            ApiResponse<BookingDto>.SuccessResponse(bookingDto, "Hotel booking created successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while creating hotel booking");
        //        return StatusCode(500, ApiResponse<BookingDto>.ErrorResponse(
        //            "An error occurred while processing your booking"));
        //    }
        //}

        //#endregion

        //#region PUT Operations

        //[HttpPut("EditHotel/{id:int}")]
        //[Authorize(Roles = "Admin,HotelManager")]
        //[ProducesResponseType(typeof(ApiResponse<HotelDto>), 200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> EditHotel(int id, HotelDto hotelDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //            return BadRequest(ApiResponse<HotelDto>.ErrorResponse("Validation failed", errors));
        //        }

        //        var existingHotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        //        if (existingHotel == null)
        //            return NotFound(ApiResponse<HotelDto>.ErrorResponse($"Hotel with ID {id} not found"));

        //        _mapper.Map(hotelDto, existingHotel);

        //        await _unitOfWork.Hotels.UpdateAsync(existingHotel);
        //        await _unitOfWork.SaveChanges();

        //        var updatedHotelDto = _mapper.Map<HotelDto>(existingHotel);
        //        return Ok(ApiResponse<HotelDto>.SuccessResponse(updatedHotelDto, "Hotel updated successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while updating hotel with ID {HotelId}", id);
        //        return StatusCode(500, ApiResponse<HotelDto>.ErrorResponse(
        //            "An error occurred while updating the hotel"));
        //    }
        //}

        //#endregion

        //#region DELETE Operations

        //[HttpDelete("DeleteHotel/{id:int}")]
        //[Authorize(Roles = "Admin")]
        //[ProducesResponseType(typeof(ApiResponse<string>), 200)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> DeleteHotel(int id)
        //{
        //    try
        //    {
        //        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        //        if (hotel == null)
        //            return NotFound(ApiResponse<string>.ErrorResponse($"Hotel with ID {id} not found"));

        //        await _unitOfWork.Hotels.DeleteAsync(hotel);
        //        await _unitOfWork.SaveChanges();

        //        return Ok(ApiResponse<string>.SuccessResponse(
        //            null, $"Hotel with ID {id} deleted successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while deleting hotel with ID {HotelId}", id);
        //        return StatusCode(500, ApiResponse<string>.ErrorResponse(
        //            "An error occurred while deleting the hotel"));
        //    }
        //}

        #endregion
    }
}
