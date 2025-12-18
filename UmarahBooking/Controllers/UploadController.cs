using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Controllers
{
    /// <summary>
    /// Controller for handling file uploads (photos, documents)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IWebHostEnvironment _environment;

        public UploadController(ILogger<UploadController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Upload a traveler photo for visa document
        /// </summary>
        /// <param name="file">Image file (JPEG or PNG, max 5MB)</param>
        /// <returns>URL of the uploaded photo</returns>
        [HttpPost("traveler-photo")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UploadTravelerPhoto(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded"));
                }

                // Validate file type - accept all common image formats  
                var allowedTypes = new[] { 
                    "image/jpeg", 
                    "image/jpg", 
                    "image/png", 
                    "image/webp",  // WebP support (frontend converts to this)
                    "image/gif",
                    "image/bmp",
                    "image/tiff"
                };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Invalid image format. Please upload JPEG, PNG, WebP, GIF, BMP, or TIFF"));
                }

                // Validate file size (max 5MB)
                const long maxSize = 5 * 1024 * 1024;
                if (file.Length > maxSize)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("File size must be less than 5MB"));
                }

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "traveler-photos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return URL
                var photoUrl = $"/uploads/traveler-photos/{uniqueFileName}";
                
                _logger.LogInformation("Traveler photo uploaded: {PhotoUrl}", photoUrl);

                return Ok(ApiResponse<string>.SuccessResponse(photoUrl, "Photo uploaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading traveler photo");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to upload photo"));
            }
        }

        /// <summary>
        /// Get a traveler photo by filename
        /// </summary>
        [HttpGet("traveler-photo/{filename}")]
        [AllowAnonymous]
        public IActionResult GetTravelerPhoto(string filename)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "traveler-photos");
                var filePath = Path.Combine(uploadsFolder, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(ApiResponse<string>.ErrorResponse("Photo not found"));
                }

                // Determine content type based on file extension
                var contentType = "image/jpeg"; // default
                if (filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";
                else if (filename.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/webp";
                else if (filename.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/gif";
                else if (filename.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/bmp";
                else if (filename.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/tiff";

                return PhysicalFile(filePath, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving traveler photo");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to retrieve photo"));
            }
        }
    }
}
