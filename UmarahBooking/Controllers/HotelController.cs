using AutoMapper;
using UmarahBooking.Core.Models;
using UmarahBooking.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;
using UmarahBooking.Data.Repositories;

namespace UmarahBooking.Controllers
{
    /// <summary>
    /// Controller for managing hotel operations including CRUD, search, and booking
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly ILogger<HotelController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHotelService _hotelService;

        public HotelController(
            ILogger<HotelController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHotelService hotelService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hotelService = hotelService;
        }

        /// <summary>
        /// Creates a new hotel (Admin or HotelManager)
        /// </summary>
        /// <param name="hotelDto">Hotel details</param>
        /// <param name="image">Optional hotel image</param>
        /// <returns>Created hotel</returns>
        [HttpPost("CreateHotel")]
        [Authorize(Roles = "Admin,HotelManager")]
        public async Task<IActionResult> CreateHotel([FromForm] HotelDto hotelDto, IFormFile? image)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<HotelDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Image Upload Handling
                if (image != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(ApiResponse<HotelDto>.ErrorResponse(
                            "Invalid file type. Only .jpg, .jpeg, .png, and .webp are allowed."));
                    }

                    // Ensure directory exists
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hotels");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    hotelDto.ImageUrl = $"/images/hotels/{uniqueFileName}";
                }
                else
                {
                    // Default image if none provided
                    hotelDto.ImageUrl = "/images/hotels/default-hotel.jpg";
                }

                using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
                try
                {
                    var hotel = _mapper.Map<Hotel>(hotelDto);
                    hotel.IsActive = true;
                    
                    // Set creator - defensive check
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int uid))
                    {
                        _logger.LogWarning("CreateHotel: User ID claim missing or invalid");
                        return Unauthorized(ApiResponse<HotelDto>.ErrorResponse(
                            "User authentication error. Please log in again."));
                    }
                    
                    hotel.CreatedByUserId = uid;

                    await _unitOfWork.Hotels.AddAsync(hotel);
                    await _unitOfWork.SaveChanges();

                    // Create rooms if provided (AutoMapper ignores Rooms on HotelDto -> Hotel mapping)
                    if (hotelDto.Rooms != null && hotelDto.Rooms.Any())
                    {
                        foreach (var roomDto in hotelDto.Rooms)
                        {
                            var room = _mapper.Map<HotelRoom>(roomDto);
                            room.HotelId = hotel.HotelId;
                            room.IsActive = roomDto.IsActive;
                            await _unitOfWork.HotelRooms.AddAsync(room);
                        }
                        await _unitOfWork.SaveChanges();
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Hotel {HotelName} created successfully with ID {HotelId}",
                        hotel.Name, hotel.HotelId);

                    // Reload hotel with rooms for response
                    var createdHotel = await _hotelService.GetHotelByIdAsync(hotel.HotelId);
                    return CreatedAtAction(
                        nameof(GetHotelById),
                        new { id = hotel.HotelId },
                        ApiResponse<HotelDto>.SuccessResponse(
                            createdHotel,
                            "Hotel created successfully"));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw; // Re-throw to be caught by outer catch
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating hotel");
                return StatusCode(500, ApiResponse<HotelDto>.ErrorResponse(
                    "An error occurred while creating the hotel"));
            }
        }

        /// <summary>
        /// Updates an existing hotel (Admin or Owner)
        /// </summary>
        /// <summary>
        /// Updates an existing hotel (Admin or Owner)
        /// </summary>
        [HttpPut("UpdateHotel/{id:int}")]
        [Authorize(Roles = "Admin,HotelManager")]
        public async Task<IActionResult> UpdateHotel(int id, [FromForm] HotelDto hotelDto, IFormFile? image)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<HotelDto>.ErrorResponse("Validation failed"));

                var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
                if (hotel == null)
                    return NotFound(ApiResponse<HotelDto>.ErrorResponse("Hotel not found"));

                // Check ownership for HotelManager
                if (User.IsInRole("HotelManager"))
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId != null && int.TryParse(userId, out int uid))
                    {
                        if (hotel.CreatedByUserId != uid)
                            return Forbid();
                    }
                }

                // Image Upload Handling
                if (image != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(ApiResponse<HotelDto>.ErrorResponse(
                            "Invalid file type. Only .jpg, .jpeg, .png, and .webp are allowed."));
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hotels");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Update ImageUrl
                    hotelDto.ImageUrl = $"/images/hotels/{uniqueFileName}";
                }
                else
                {
                    // Keep existing image if not provided
                    hotelDto.ImageUrl = hotel.ImageUrl;
                }

                // Update hotel properties
                hotel.Name = hotelDto.Name;
                // Safe enum parsing for City
                if (Enum.TryParse<HotelCity>(hotelDto.City, true, out var parsedCity))
                {
                    hotel.HotelCity = parsedCity;
                }
                else
                {
                    hotel.HotelCity = HotelCity.Makkah; // Default value
                }
                hotel.Address = hotelDto.Address;
                hotel.StarRating = hotelDto.StarRating;
                hotel.DistanceToHaram = hotelDto.DistanceToHaram;
                hotel.Description = hotelDto.Description;
                hotel.ImageUrl = hotelDto.ImageUrl;

                // Handle Rooms - Load existing rooms first
                var existingRooms = await _unitOfWork.HotelRooms.FindAllBySearch(r => r.HotelId == id);

                if (hotelDto.Rooms != null && hotelDto.Rooms.Any())
                {
                    // Get IDs of rooms in the update request
                    var incomingRoomIds = hotelDto.Rooms
                        .Where(r => r.Id > 0)
                        .Select(r => r.Id)
                        .ToList();

                    // Remove rooms that are not in the update request
                    var roomsToRemove = existingRooms
                        .Where(r => !incomingRoomIds.Contains(r.HotelRoomId))
                        .ToList();

                    foreach (var room in roomsToRemove)
                    {
                        await _unitOfWork.HotelRooms.DeleteAsync(room);
                    }

                    // Update or add rooms
                    foreach (var roomDto in hotelDto.Rooms)
                    {
                        if (roomDto.Id > 0)
                        {
                            // Update existing room
                            var existingRoom = existingRooms.FirstOrDefault(r => r.HotelRoomId == roomDto.Id);
                            if (existingRoom != null)
                            {
                                // Safe enum parsing for RoomType
                                if (Enum.TryParse<RoomType>(roomDto.RoomType, true, out var parsedRoomType))
                                {
                                    existingRoom.RoomType = parsedRoomType;
                                }
                                existingRoom.Capacity = roomDto.Capacity;
                                existingRoom.PricePerNight = roomDto.PricePerNight;
                                existingRoom.AvailableRooms = roomDto.AvailableRooms;
                                existingRoom.IsActive = roomDto.IsActive;
                                await _unitOfWork.HotelRooms.UpdateAsync(existingRoom);
                            }
                        }
                        else
                        {
                            // Add new room
                            var newRoom = new HotelRoom
                            {
                                HotelId = id,
                                RoomType = Enum.TryParse<RoomType>(roomDto.RoomType, true, out var newRoomType) ? newRoomType : RoomType.Single,
                                Capacity = roomDto.Capacity,
                                PricePerNight = roomDto.PricePerNight,
                                AvailableRooms = roomDto.AvailableRooms,
                                IsActive = roomDto.IsActive
                            };
                            await _unitOfWork.HotelRooms.AddAsync(newRoom);

                            _logger.LogInformation(
                                "Adding new room {RoomType} to hotel {HotelId}",
                                newRoom.RoomType, id);
                        }
                    }
                }

                await _unitOfWork.Hotels.UpdateAsync(hotel);
                await _unitOfWork.SaveChanges();

                // Reload hotel with rooms for response
                var updatedHotel = await _hotelService.GetHotelByIdAsync(id);
                return Ok(ApiResponse<HotelDto>.SuccessResponse(updatedHotel, "Hotel updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel {HotelId}", id);
                return StatusCode(500, ApiResponse<HotelDto>.ErrorResponse("Error updating hotel"));
            }
        }

        /// <summary>
        /// Deletes a hotel (Admin or Owner)
        /// </summary>
        [HttpDelete("DeleteHotel/{id:int}")]
        [Authorize(Roles = "Admin,HotelManager")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            try
            {
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
                if (hotel == null)
                    return NotFound(ApiResponse<string>.ErrorResponse("Hotel not found"));

                // Check ownership for HotelManager
                if (User.IsInRole("HotelManager"))
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId != null && int.TryParse(userId, out int uid))
                    {
                        if (hotel.CreatedByUserId != uid)
                            return Forbid();
                    }
                }

                // Soft delete
                hotel.IsActive = false;
                await _unitOfWork.Hotels.UpdateAsync(hotel);
                await _unitOfWork.SaveChanges();

                return Ok(ApiResponse<string>.SuccessResponse(null, "Hotel deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel {HotelId}", id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Error deleting hotel"));
            }
        }

        /// <summary>
        /// Gets hotels created by the current user (HotelManager)
        /// </summary>
        [HttpGet("GetMyHotels")]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> GetMyHotels()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                    return Unauthorized(ApiResponse<IEnumerable<HotelDto>>.ErrorResponse("User not found"));

                // Include Rooms in the query so they appear in edit form
                var hotels = await _unitOfWork.Hotels.FindWithAsync(
                    h => h.CreatedByUserId == userId && h.IsActive,
                    new[] { "Rooms" });
                var dtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);

                return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResponse(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving my hotels");
                return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResponse("Error retrieving hotels"));
            }
        }

        [HttpGet("GetAllFiltered")]
        public async Task<IActionResult> GetAllFiltered([FromQuery] string? city, [FromQuery] string? filter)
        {
            try
            {
                var hotels = await _hotelService.GetFilteredHotelsAsync(city, filter);
                // Always return 200 with data (empty list if no matches)
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
    }
}

