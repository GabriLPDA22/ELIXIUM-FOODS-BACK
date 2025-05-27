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

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
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
                if (file == null || file.Length == 0)
                    return BadRequest("No image file provided");

                var result = await _imageService.UploadImageAsync(file, folder);
                
                return Ok(new { 
                    success = true,
                    imageUrl = result.ImageUrl,
                    fileName = result.FileName,
                    size = result.Size
                });
            }
            catch (Exception ex)
            {
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
                if (string.IsNullOrEmpty(request.Base64Image))
                    return BadRequest("No image data provided");

                var result = await _imageService.UploadImageBase64Async(
                    request.Base64Image, 
                    request.FileName ?? "image", 
                    request.Folder ?? "general"
                );
                
                return Ok(new { 
                    success = true,
                    imageUrl = result.ImageUrl,
                    fileName = result.FileName,
                    size = result.Size
                });
            }
            catch (Exception ex)
            {
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
                if (string.IsNullOrEmpty(imageUrl))
                    return BadRequest("Image URL is required");

                var success = await _imageService.DeleteImageAsync(imageUrl);
                
                return Ok(new { 
                    success,
                    message = success ? "Image deleted successfully" : "Image not found or already deleted"
                });
            }
            catch (Exception ex)
            {
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
                if (files == null || files.Count == 0)
                    return BadRequest("No image files provided");

                var results = await _imageService.UploadMultipleImagesAsync(files, folder);
                
                return Ok(new { 
                    success = true,
                    images = results,
                    count = results.Count
                });
            }
            catch (Exception ex)
            {
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