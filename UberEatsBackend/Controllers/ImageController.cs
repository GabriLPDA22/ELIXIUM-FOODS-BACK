// UberEatsBackend/Controllers/ImageController.cs
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// Sube una imagen usando IFormFile
        /// </summary>
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "general")
        {
            try
            {
                _logger.LogInformation($"Attempting to upload file: {file?.FileName}");
                
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No image file provided");
                    return BadRequest(new { 
                        success = false,
                        message = "No image file provided" 
                    });
                }

                var result = await _imageService.UploadImageAsync(file, folder);
                
                _logger.LogInformation($"Image uploaded successfully: {result.ImageUrl}");
                
                return Ok(new { 
                    success = true,
                    imageUrl = result.ImageUrl,
                    fileName = result.FileName,
                    size = result.Size
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image via form file");
                return StatusCode(500, new { 
                    success = false,
                    message = $"Error uploading image: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Sube una imagen usando base64
        /// </summary>
        [HttpPost("upload/base64")]
        [Authorize]
        public async Task<IActionResult> UploadImageBase64([FromBody] UploadImageBase64Request request)
        {
            try
            {
                _logger.LogInformation($"Attempting to upload base64 image: {request?.FileName}");
                
                if (request == null || string.IsNullOrEmpty(request.Base64Image))
                {
                    _logger.LogWarning("No image data provided in base64 upload");
                    return BadRequest(new { 
                        success = false,
                        message = "No image data provided" 
                    });
                }

                var result = await _imageService.UploadImageBase64Async(
                    request.Base64Image, 
                    request.FileName ?? "image", 
                    request.Folder ?? "general"
                );
                
                _logger.LogInformation($"Base64 image uploaded successfully: {result.ImageUrl}");
                
                return Ok(new { 
                    success = true,
                    imageUrl = result.ImageUrl,
                    fileName = result.FileName,
                    size = result.Size
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid image data provided");
                return BadRequest(new { 
                    success = false,
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading base64 image");
                return StatusCode(500, new { 
                    success = false,
                    message = $"Error uploading image: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Elimina una imagen
        /// </summary>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete image: {imageUrl}");
                
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return BadRequest(new { 
                        success = false,
                        message = "Image URL is required" 
                    });
                }

                var success = await _imageService.DeleteImageAsync(imageUrl);
                
                _logger.LogInformation($"Image deletion result: {success}");
                
                return Ok(new { 
                    success,
                    message = success ? "Image deleted successfully" : "Image not found or already deleted"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {imageUrl}");
                return StatusCode(500, new { 
                    success = false,
                    message = $"Error deleting image: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Sube múltiples imágenes
        /// </summary>
        [HttpPost("upload/multiple")]
        [Authorize]
        public async Task<IActionResult> UploadMultipleImages(IFormFileCollection files, [FromQuery] string folder = "general")
        {
            try
            {
                _logger.LogInformation($"Attempting to upload {files?.Count ?? 0} files");
                
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { 
                        success = false,
                        message = "No image files provided" 
                    });
                }

                var results = await _imageService.UploadMultipleImagesAsync(files, folder);
                
                _logger.LogInformation($"Multiple images uploaded successfully: {results.Count}");
                
                return Ok(new { 
                    success = true,
                    images = results,
                    count = results.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple images");
                return StatusCode(500, new { 
                    success = false,
                    message = $"Error uploading images: {ex.Message}" 
                });
            }
        }
    }

    public class UploadImageBase64Request
    {
        public string Base64Image { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? Folder { get; set; }
    }
}