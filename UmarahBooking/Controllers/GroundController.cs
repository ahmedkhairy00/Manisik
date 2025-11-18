using AutoMapper;
using Manisik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using UmarahBooking.Core.DTO;
using UmarahBooking.Core.Interfaces;

namespace UmarahBooking.Controllers
{
    /// <summary>
    /// Controller for managing ground transportation services (buses, private cars, etc.)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GroundTransportController : ControllerBase
    {
        #region Dependencies

        private readonly ILogger<GroundTransportController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public GroundTransportController(
            ILogger<GroundTransportController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Retrieves all ground transport services
        /// </summary>
        /// <returns>List of all ground transports</returns>
        [HttpGet("GetAllGroundTransports")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<GroundTransportDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllGroundTransports()
        {
            try
            {
                // Fetch all ground transport services
                var transports = await _unitOfWork.GroundTransports.GetAllAsync();

                // Map to DTOs
                var transportDtos = _mapper.Map<IEnumerable<GroundTransportDto>>(transports);

                return Ok(ApiResponse<IEnumerable<GroundTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"{transports.Count()} ground transport services retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all ground transports");
                return StatusCode(500, ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                    "An error occurred while retrieving ground transport services"));
            }
        }

        /// <summary>
        /// Retrieves a specific ground transport by ID
        /// </summary>
        /// <param name="id">Ground transport ID</param>
        /// <returns>Ground transport details</returns>
        [HttpGet("GetGroundTransportById/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GroundTransportDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGroundTransportById(int id)
        {
            try
            {
                // Fetch ground transport by ID
                var transport = await _unitOfWork.GroundTransports.GetByIdAsync(id);

                if (transport == null)
                {
                    _logger.LogWarning("Ground transport with ID {TransportId} not found", id);
                    return NotFound(ApiResponse<GroundTransportDto>.ErrorResponse(
                        $"Ground transport with ID {id} not found"));
                }

                var transportDto = _mapper.Map<GroundTransportDto>(transport);
                return Ok(ApiResponse<GroundTransportDto>.SuccessResponse(
                    transportDto,
                    "Ground transport retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ground transport with ID {TransportId}", id);
                return StatusCode(500, ApiResponse<GroundTransportDto>.ErrorResponse(
                    "An error occurred while retrieving the ground transport"));
            }
        }

        /// <summary>
        /// Search ground transports by type (Bus, PrivateCar, Shuttle, etc.)
        /// </summary>
        /// <param name="transportType">Type of internal transport</param>
        /// <returns>List of matching transports</returns>
        [HttpGet("SearchByType")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<GroundTransportDto>>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByType(
            [FromQuery] Manisik.Enums.InternalTransportType transportType)
        {
            try
            {
                // Search for transports by type
                var transports = await _unitOfWork.GroundTransports.FindAllBySearch(
                    t => t.InternalTransportType == transportType && t.IsActive);

                if (!transports.Any())
                {
                    _logger.LogInformation("No ground transports found of type {TransportType}", transportType);
                    return NotFound(ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                        $"No ground transports available of type {transportType}"));
                }

                var transportDtos = _mapper.Map<IEnumerable<GroundTransportDto>>(transports);
                return Ok(ApiResponse<IEnumerable<GroundTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"{transports.Count()} ground transports found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching ground transports by type");
                return StatusCode(500, ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                    "An error occurred while searching ground transports"));
            }
        }

        /// <summary>
        /// Search ground transports by capacity range
        /// </summary>
        /// <param name="minCapacity">Minimum capacity</param>
        /// <param name="maxCapacity">Maximum capacity</param>
        /// <returns>List of transports within capacity range</returns>
        [HttpGet("SearchByCapacity")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<GroundTransportDto>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByCapacity(
            [FromQuery] int minCapacity = 1,
            [FromQuery] int maxCapacity = 100)
        {
            try
            {
                // Validate capacity range
                if (minCapacity < 1 || maxCapacity < minCapacity)
                {
                    return BadRequest(ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                        "Invalid capacity range"));
                }

                // Search transports within capacity range
                var transports = await _unitOfWork.GroundTransports.FindAllBySearch(
                    t => t.Capacity >= minCapacity &&
                         t.Capacity <= maxCapacity &&
                         t.IsActive);

                if (!transports.Any())
                {
                    return NotFound(ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                        $"No ground transports available with capacity between {minCapacity} and {maxCapacity}"));
                }

                var transportDtos = _mapper.Map<IEnumerable<GroundTransportDto>>(transports);
                return Ok(ApiResponse<IEnumerable<GroundTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"{transports.Count()} ground transports found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching ground transports by capacity");
                return StatusCode(500, ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                    "An error occurred while searching ground transports"));
            }
        }

        /// <summary>
        /// Advanced search with filtering, pagination, and sorting
        /// </summary>
        /// <param name="serviceName">Service name filter (optional)</param>
        /// <param name="transportType">Transport type filter (optional)</param>
        /// <param name="minPrice">Minimum price per person (optional)</param>
        /// <param name="maxPrice">Maximum price per person (optional)</param>
        /// <param name="minCapacity">Minimum capacity (optional)</param>
        /// <param name="take">Number of records to return</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="orderBy">Field to sort by</param>
        /// <param name="orderDirection">Sort direction</param>
        /// <returns>Filtered and paginated ground transports</returns>
        [HttpGet("AdvancedSearch")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<GroundTransportDto>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AdvancedSearch(
            [FromQuery] string? serviceName = null,
            [FromQuery] Manisik.Enums.InternalTransportType? transportType = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? minCapacity = null,
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0,
            [FromQuery] string orderBy = "ServiceName",
            [FromQuery] string orderDirection = "Asc")
        {
            try
            {
                // Validate pagination parameters
                if (take <= 0 || take > 100)
                {
                    return BadRequest(ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                        "Take value must be between 1 and 100"));
                }

                if (skip < 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                        "Skip value cannot be negative"));
                }

                // Validate price range
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    return BadRequest(ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                        "Minimum price cannot be greater than maximum price"));
                }

                // Build dynamic ordering expression
                Expression<Func<GroundTransport, object>> orderExpression = orderBy?.ToLower() switch
                {
                    "servicename" or "name" => t => t.ServiceName!,
                    "price" or "priceperperson" => t => t.PricePerPerson,
                    "capacity" => t => t.Capacity,
                    "type" or "transporttype" => t => t.InternalTransportType,
                    _ => t => t.ServiceName! // Default
                };

                // Determine sort direction
                var sortDirection = orderDirection?.ToLower() == "asc"
                    ? Core.Const.OrderBy.Ascending
                    : Core.Const.OrderBy.Descending;

                // Build complex filter criteria
                Expression<Func<GroundTransport, bool>> criteria = t =>
                    t.IsActive &&
                    (string.IsNullOrEmpty(serviceName) || t.ServiceName!.Contains(serviceName)) &&
                    (!transportType.HasValue || t.InternalTransportType == transportType.Value) &&
                    (!minPrice.HasValue || t.PricePerPerson >= minPrice.Value) &&
                    (!maxPrice.HasValue || t.PricePerPerson <= maxPrice.Value) &&
                    (!minCapacity.HasValue || t.Capacity >= minCapacity.Value);

                // Execute search
                var transports = await _unitOfWork.GroundTransports
                    .FindAllBySearchAndSkipWithOrder(
                        criteria: criteria,
                        take: take,
                        skip: skip,
                        orderBy: orderExpression,
                        orderByDirection: sortDirection);

                var transportDtos = _mapper.Map<IEnumerable<GroundTransportDto>>(transports);

                return Ok(ApiResponse<IEnumerable<GroundTransportDto>>.SuccessResponse(
                    transportDtos,
                    $"Retrieved {transports.Count()} ground transports (Page {(skip / take) + 1})"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during advanced ground transport search");
                return StatusCode(500, ApiResponse<IEnumerable<GroundTransportDto>>.ErrorResponse(
                    "An error occurred while searching ground transports"));
            }
        }

        #endregion

        #region POST Operations

        /// <summary>
        /// Creates a new ground transport service (Admin only)
        /// </summary>
        /// <param name="transportDto">Ground transport details</param>
        /// <returns>Created ground transport</returns>
        [HttpPost("CreateGroundTransport")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<GroundTransportDto>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateGroundTransport([FromBody] GroundTransportDto transportDto)
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

                    return BadRequest(ApiResponse<GroundTransportDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Business validation
                if (transportDto.PricePerPerson <= 0)
                {
                    return BadRequest(ApiResponse<GroundTransportDto>.ErrorResponse(
                        "Price per person must be greater than zero"));
                }

                if (transportDto.Capacity <= 0)
                {
                    return BadRequest(ApiResponse<GroundTransportDto>.ErrorResponse(
                        "Capacity must be greater than zero"));
                }

                // Map DTO to entity
                var transport = _mapper.Map<GroundTransport>(transportDto);
                transport.IsActive = true;

                // Save to database
                await _unitOfWork.GroundTransports.AddAsync(transport);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation(
                    "Ground transport service {ServiceName} created successfully with ID {TransportId}",
                    transport.ServiceName, transport.GroundTransportId);

                var createdTransportDto = _mapper.Map<GroundTransportDto>(transport);
                return CreatedAtAction(
                    nameof(GetGroundTransportById),
                    new { id = transport.GroundTransportId },
                    ApiResponse<GroundTransportDto>.SuccessResponse(
                        createdTransportDto,
                        "Ground transport service created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating ground transport");
                return StatusCode(500, ApiResponse<GroundTransportDto>.ErrorResponse(
                    "An error occurred while creating the ground transport service"));
            }
        }

        #endregion

        #region PUT Operations

        /// <summary>
        /// Updates an existing ground transport service (Admin only)
        /// </summary>
        /// <param name="id">Ground transport ID</param>
        /// <param name="transportDto">Updated ground transport details</param>
        /// <returns>Updated ground transport</returns>
        [HttpPut("UpdateGroundTransport/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<GroundTransportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateGroundTransport(int id, [FromBody] GroundTransportDto transportDto)
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

                    return BadRequest(ApiResponse<GroundTransportDto>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Check if ground transport exists
                var existingTransport = await _unitOfWork.GroundTransports.GetByIdAsync(id);
                if (existingTransport == null)
                {
                    _logger.LogWarning("Attempted to update non-existent ground transport with ID {TransportId}", id);
                    return NotFound(ApiResponse<GroundTransportDto>.ErrorResponse(
                        $"Ground transport with ID {id} not found"));
                }

                // Business validation
                if (transportDto.PricePerPerson <= 0)
                {
                    return BadRequest(ApiResponse<GroundTransportDto>.ErrorResponse(
                        "Price per person must be greater than zero"));
                }

                if (transportDto.Capacity <= 0)
                {
                    return BadRequest(ApiResponse<GroundTransportDto>.ErrorResponse(
                        "Capacity must be greater than zero"));
                }

                // Map updates to existing entity
                _mapper.Map(transportDto, existingTransport);

                // Update in database
                await _unitOfWork.GroundTransports.UpdateAsync(existingTransport);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation("Ground transport {TransportId} updated successfully", id);

                var updatedTransportDto = _mapper.Map<GroundTransportDto>(existingTransport);
                return Ok(ApiResponse<GroundTransportDto>.SuccessResponse(
                    updatedTransportDto,
                    "Ground transport service updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating ground transport with ID {TransportId}", id);
                return StatusCode(500, ApiResponse<GroundTransportDto>.ErrorResponse(
                    "An error occurred while updating the ground transport service"));
            }
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Deletes a ground transport service (Admin only)
        /// </summary>
        /// <param name="id">Ground transport ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("DeleteGroundTransport/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteGroundTransport(int id)
        {
            try
            {
                // Check if ground transport exists
                var transport = await _unitOfWork.GroundTransports.GetByIdAsync(id);
                if (transport == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent ground transport with ID {TransportId}", id);
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        $"Ground transport with ID {id} not found"));
                }

                // Soft delete (recommended if there are bookings)
                transport.IsActive = false;
                await _unitOfWork.GroundTransports.UpdateAsync(transport);

                // Or hard delete (uncomment if preferred):
                // await _unitOfWork.GroundTransports.DeleteAsync(transport);

                await _unitOfWork.SaveChanges();

                _logger.LogInformation("Ground transport {TransportId} deleted successfully", id);

                return Ok(ApiResponse<string>.SuccessResponse(
                    null,
                    $"Ground transport with ID {id} deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting ground transport with ID {TransportId}", id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    "An error occurred while deleting the ground transport service"));
            }
        }

        #endregion
    }
}