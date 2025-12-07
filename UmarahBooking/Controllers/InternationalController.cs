using AutoMapper;
using Manisik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;
using UmarahBooking.Core.Services;

namespace UmarahBooking.Controllers
{
    /// <summary>
    /// Controller for managing international transport (flights/ships) operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InternationalTransportController : ControllerBase
    {
        #region Dependencies

        private readonly ILogger<InternationalTransportController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
       
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public InternationalTransportController(
            ILogger<InternationalTransportController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper
           
            )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Retrieves all international transport options
        /// </summary>
        /// <returns>List of all transports</returns>
        [HttpGet("GetAllTransports")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<InternationalTransportDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllTransports()
        {
            try
            {
                // Fetch all active transport options
                var transports = await _unitOfWork.InternationalTransports.FindAllBySearch(t => t.IsActive);

                // Map to DTOs
                var transportDtos = _mapper.Map<IEnumerable<InternationalTransportDto>>(transports);

                return Ok(ApiResponse<IEnumerable<InternationalTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"{transports.Count()} international transport options retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all international transports");
                return StatusCode(500, ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                    "An error occurred while retrieving transport options"));
            }
        }

        /// <summary>
        /// Retrieves a specific transport by ID
        /// </summary>
        /// <param name="id">Transport ID</param>
        /// <returns>Transport details</returns>
        [HttpGet("GetTransportById/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<InternationalTransportDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTransportById(int id)
        {
            try
            {
                // Fetch transport by ID
                var transport = await _unitOfWork.InternationalTransports.GetByIdAsync(id);

                if (transport == null)
                {
                    _logger.LogWarning("International transport with ID {TransportId} not found", id);
                    return NotFound(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        $"Transport with ID {id} not found"));
                }

                var transportDto = _mapper.Map<InternationalTransportDto>(transport);
                return Ok(ApiResponse<InternationalTransportDto>.SuccessResponse(
                    transportDto,
                    "Transport retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving transport with ID {TransportId}", id);
                return StatusCode(500, ApiResponse<InternationalTransportDto>.ErrorResponse(
                    "An error occurred while retrieving the transport"));
            }
        }

        /// <summary>
        /// Search transports by route (departure and arrival airports)
        /// </summary>
        /// <param name="departureAirport">Departure airport enum value</param>
        /// <param name="arrivalAirport">Arrival airport enum value</param>
        /// <returns>List of matching transports</returns>
        [HttpGet("SearchByRoute")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<InternationalTransportDto>>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByRoute(
            [FromQuery] string departureAirport,
            [FromQuery] string arrivalAirport)
        {
            try
            {
                // Search for transports matching the route
                var transports = await _unitOfWork.InternationalTransports.FindAllBySearch(
                    t => t.DepartureAirport.ToString() == departureAirport &&
                    t.ArrivalAirport.ToString() == arrivalAirport &&
                    t.IsActive
                    );

                if (!transports.Any())
                {
                    _logger.LogInformation(
                        "No transports found from {Departure} to {Arrival}",
                        departureAirport, arrivalAirport);

                    return NotFound(ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                        $"No transports available from {departureAirport} to {arrivalAirport}"));
                }

                var transportDtos = _mapper.Map<IEnumerable<InternationalTransportDto>>(transports);
                return Ok(ApiResponse<IEnumerable<InternationalTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"{transports.Count()} transports found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching transports by route");
                return StatusCode(500, ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                    "An error occurred while searching transports"));
            }
        }

        /// <summary>
        /// Search transports by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of transports within date range</returns>
        [HttpGet("SearchByDateRange")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<InternationalTransportDto>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime returnDate)
        {
            try
            {
                // Validate date range
                if (startDate >= returnDate)
                {
                    return BadRequest(ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                        "Start date must be before end date"));
                }

                // Search transports within date range
                //var transports = await _unitOfWork.InternationalTransports.FindAllBySearch(
                //    t => t.DepartureDate.Date == startDate.Date &&
                //         t.ReturnDate.Value.Date == returnDate.Date &&
                //         t.IsActive);
                var transports = await _unitOfWork.InternationalTransports.FindAllBySearch(
                t => t.DepartureDate >= startDate.Date && t.DepartureDate < startDate.Date.AddDays(1) &&
                     t.ReturnDate.HasValue &&
                     t.ReturnDate.Value >= returnDate.Date && t.ReturnDate.Value < returnDate.Date.AddDays(1) &&
                     t.IsActive);

                if (!transports.Any())
                {
                    return NotFound(ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                        $"No transports available in {startDate:yyyy-MM-dd} and {returnDate:yyyy-MM-dd}"));
                }

                var transportDtos = _mapper.Map<IEnumerable<InternationalTransportDto>>(transports);
                return Ok(ApiResponse<IEnumerable<InternationalTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"{transports.Count()} transports found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching transports by date range");
                return StatusCode(500, ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                    "An error occurred while searching transports"));
            }
        }

        /// <summary>
        /// Advanced search with filtering, pagination, and sorting
        /// </summary>
        /// <param name="carrierName">Carrier name filter (optional)</param>
        /// <param name="transportType">Transport type filter (optional)</param>
        /// <param name="minPrice">Minimum price (optional)</param>
        /// <param name="maxPrice">Maximum price (optional)</param>
        /// <param name="take">Number of records to return</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="orderBy">Field to sort by</param>
        /// <param name="orderDirection">Sort direction</param>
        /// <returns>Filtered and paginated transports</returns>
        [HttpGet("AdvancedSearch")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<InternationalTransportDto>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AdvancedSearch(
            [FromQuery] string? carrierName = null,
            [FromQuery] Manisik.Enums.InternationalTransportType? transportType = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0,
            [FromQuery] string orderBy = "DepartureDate",
            [FromQuery] string orderDirection = "Asc")
        {
            try
            {
                // Validate pagination parameters
                if (take <= 0 || take > 100)
                {
                    return BadRequest(ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                        "Take value must be between 1 and 100"));
                }

                if (skip < 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                        "Skip value cannot be negative"));
                }

                // Validate price range
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    return BadRequest(ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                        "Minimum price cannot be greater than maximum price"));
                }

                // Build dynamic ordering expression
                Expression<Func<InternationalTransport, object>> orderExpression = orderBy?.ToLower() switch
                {
                    "carrier" or "carriername" => t => t.CarrierName!,
                    "price" => t => t.Price,
                    "departuredate" => t => t.DepartureDate,
                    "availableseats" => t => t.AvailableSeats,
                    _ => t => t.DepartureDate // Default
                };

                // Determine sort direction
                var sortDirection = orderDirection?.ToLower() == "asc"
                    ? Core.Const.OrderBy.Ascending
                    : Core.Const.OrderBy.Descending;

                // Build complex filter criteria
                Expression<Func<InternationalTransport, bool>> criteria = t =>
                    t.IsActive &&
                    (string.IsNullOrEmpty(carrierName) || t.CarrierName.Contains(carrierName)) &&
                    (!transportType.HasValue || t.TransportType == transportType.Value) &&
                    (!minPrice.HasValue || t.Price >= minPrice.Value) &&
                    (!maxPrice.HasValue || t.Price <= maxPrice.Value);

                // Execute search
                var transports = await _unitOfWork.InternationalTransports
                    .FindAllBySearchAndSkipWithOrder(
                        criteria: criteria,
                        take: take,
                        skip: skip,
                        orderBy: orderExpression,
                        orderByDirection: sortDirection);

                var transportDtos = _mapper.Map<IEnumerable<InternationalTransportDto>>(transports);

                return Ok(ApiResponse<IEnumerable<InternationalTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"Retrieved {transports.Count()} transports (Page {(skip / take) + 1})"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during advanced transport search");
                return StatusCode(500, ApiResponse<IEnumerable<InternationalTransportDto>>.ErrorResponse(
                    "An error occurred while searching transports"));
            }
        }

        #endregion

        #region POST Operations

        /// <summary>
        /// Creates a new international transport (Admin only)
        /// </summary>
        /// <param name="transportDto">Transport details</param>
        /// <returns>Created transport</returns>
        [HttpPost("CreateTransport")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<InternationalTransportDto>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateTransport([FromBody] InternationalTransportDto transportDto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Business validation: Arrival date must be after departure
                if (transportDto.ArrivalDate <= transportDto.DepartureDate)
                {
                    return BadRequest(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        "Arrival date must be after departure date"));
                }

                // Business validation: Departure date must be in the future
                if (transportDto.DepartureDate < DateTime.UtcNow)
                {
                    return BadRequest(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        "Departure date must be in the future"));
                }

                // Map DTO to entity
                var transport = _mapper.Map<InternationalTransport>(transportDto);
                transport.IsActive = true;

                // Save to database
                await _unitOfWork.InternationalTransports.AddAsync(transport);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "International transport {CarrierName} created successfully with ID {TransportId}",
                    transport.CarrierName, transport.InternationalTransportId);

                var createdTransportDto = _mapper.Map<InternationalTransportDto>(transport);
                return CreatedAtAction(
                    nameof(GetTransportById),
                    new { id = transport.InternationalTransportId },
                    ApiResponse<InternationalTransportDto>.SuccessResponse(
                        createdTransportDto,
                        "Transport created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating international transport");
                return StatusCode(500, ApiResponse<InternationalTransportDto>.ErrorResponse(
                    "An error occurred while creating the transport"));
            }
        }

        #endregion

        #region PUT Operations

        /// <summary>
        /// Updates an existing international transport (Admin only)
        /// </summary>
        /// <param name="id">Transport ID</param>
        /// <param name="transportDto">Updated transport details</param>
        /// <returns>Updated transport</returns>
        [HttpPut("UpdateTransport/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<InternationalTransportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateTransport(int id, [FromBody] InternationalTransportDto transportDto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Check if transport exists
                var existingTransport = await _unitOfWork.InternationalTransports.GetByIdAsync(id);
                if (existingTransport == null)
                {
                    _logger.LogWarning("Attempted to update non-existent transport with ID {TransportId}", id);
                    return NotFound(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        $"Transport with ID {id} not found"));
                }

                // Business validation
                if (transportDto.ArrivalDate <= transportDto.DepartureDate)
                {
                    return BadRequest(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        "Arrival date must be after departure date"));
                }

                // Map updates to existing entity
                _mapper.Map(transportDto, existingTransport);

                // Update in database
                await _unitOfWork.InternationalTransports.UpdateAsync(existingTransport);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation("International transport {TransportId} updated successfully", id);

                var updatedTransportDto = _mapper.Map<InternationalTransportDto>(existingTransport);
                return Ok(ApiResponse<InternationalTransportDto>.SuccessResponse(
                    updatedTransportDto,
                    "Transport updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating transport with ID {TransportId}", id);
                return StatusCode(500, ApiResponse<InternationalTransportDto>.ErrorResponse(
                    "An error occurred while updating the transport"));
            }
        }

        /// <summary>
        /// Updates available seats (Admin only) - used after bookings
        /// </summary>
        /// <param name="id">Transport ID</param>
        /// <param name="seatsBooked">Number of seats to deduct</param>
        /// <returns>Updated transport</returns>
        [HttpPatch("UpdateAvailableSeats/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<InternationalTransportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateAvailableSeats(int id, [FromBody] int seatsBooked)
        {
            try
            {
                // Validate seats booked
                if (seatsBooked <= 0)
                {
                    return BadRequest(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        "Seats booked must be greater than zero"));
                }

                // Get transport
                var transport = await _unitOfWork.InternationalTransports.GetByIdAsync(id);
                if (transport == null)
                {
                    return NotFound(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        $"Transport with ID {id} not found"));
                }

                // Check availability
                if (transport.AvailableSeats < seatsBooked)
                {
                    return BadRequest(ApiResponse<InternationalTransportDto>.ErrorResponse(
                        $"Not enough seats available. Only {transport.AvailableSeats} seats remaining"));
                }

                // Update available seats
                transport.AvailableSeats -= seatsBooked;

                await _unitOfWork.InternationalTransports.UpdateAsync(transport);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Updated seats for transport {TransportId}. Booked: {SeatsBooked}, Remaining: {RemainingSeats}",
                    id, seatsBooked, transport.AvailableSeats);

                var transportDto = _mapper.Map<InternationalTransportDto>(transport);
                return Ok(ApiResponse<InternationalTransportDto>.SuccessResponse(
                    transportDto,
                    $"Successfully booked {seatsBooked} seats"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating seats for transport {TransportId}", id);
                return StatusCode(500, ApiResponse<InternationalTransportDto>.ErrorResponse(
                    "An error occurred while updating seat availability"));
            }
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Deletes an international transport (Admin only)
        /// </summary>
        /// <param name="id">Transport ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("DeleteTransport/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteTransport(int id)
        {
            try
            {
                // Check if transport exists
                var transport = await _unitOfWork.InternationalTransports.GetByIdAsync(id);
                if (transport == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent transport with ID {TransportId}", id);
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        $"Transport with ID {id} not found"));
                }

                // Soft delete (recommended if there are bookings)
                transport.IsActive = false;
                await _unitOfWork.InternationalTransports.UpdateAsync(transport);

                // Or hard delete (uncomment if preferred):
                // await _unitOfWork.InternationalTransports.DeleteAsync(transport);

                await _unitOfWork.SaveChanges();

                _logger.LogInformation("International transport {TransportId} deleted successfully", id);

                return Ok(ApiResponse<string>.SuccessResponse(
                    string.Empty,
                    $"Transport with ID {id} deleted successfully. The transport is no longer available for booking."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting transport with ID {TransportId}", id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    "An error occurred while deleting the transport"));
            }
        }

        #endregion
    }
}